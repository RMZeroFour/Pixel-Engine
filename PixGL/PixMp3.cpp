// Taken and modified from https://docs.microsoft.com/en-us/windows/desktop/medfound/tutorial--decoding-audio  

#define WINVER _WIN32_WINNT_WIN7

#ifndef _UNICODE
#define _UNICODE
#endif

#include <windows.h>
#include <mfapi.h>
#include <mfidl.h>
#include <mfreadwrite.h>
#include <stdio.h>
#include <mferror.h>
#include "PixMp3.h"

#pragma comment(lib, "mfplat.lib")
#pragma comment(lib, "mfreadwrite.lib")
#pragma comment(lib, "mfuuid.lib")

template <class T> void SafeRelease(T **ppT)
{
	if (*ppT)
	{
		(*ppT)->Release();
		*ppT = NULL;
	}
}

//-------------------------------------------------------------------
//
// Writes a block of data to a file
//
// hFile: Handle to the file.
// p: Pointer to the buffer to write.
// cb: Size of the buffer, in bytes.
//
//-------------------------------------------------------------------
HRESULT WriteToFile(HANDLE hFile, void* p, DWORD cb)
{
	DWORD cbWritten = 0;
	HRESULT hr = S_OK;

	BOOL bResult = WriteFile(hFile, p, cb, &cbWritten, NULL);
	if (!bResult)
	{
		hr = HRESULT_FROM_WIN32(GetLastError());
	}
	return hr;
}

//-------------------------------------------------------------------
// FixUpChunkSizes
//
// Writes the file-size information into the WAVE file header.
//
// WAVE files use the RIFF file format. Each RIFF chunk has a data
// size, and the RIFF header has a total file size.
//-------------------------------------------------------------------
HRESULT FixUpChunkSizes(HANDLE hFile, DWORD cbHeader, DWORD cbAudioData)
{
	HRESULT hr = S_OK;

	LARGE_INTEGER ll;
	ll.QuadPart = cbHeader - sizeof(DWORD);

	if (0 == SetFilePointerEx(hFile, ll, NULL, FILE_BEGIN))
	{
		hr = HRESULT_FROM_WIN32(GetLastError());
	}

	// Write the data size.

	if (SUCCEEDED(hr))
	{
		hr = WriteToFile(hFile, &cbAudioData, sizeof(cbAudioData));
	}

	if (SUCCEEDED(hr))
	{
		// Write the file size.
		ll.QuadPart = sizeof(FOURCC);

		if (0 == SetFilePointerEx(hFile, ll, NULL, FILE_BEGIN))
		{
			hr = HRESULT_FROM_WIN32(GetLastError());
		}
	}

	if (SUCCEEDED(hr))
	{
		DWORD cbRiffFileSize = cbHeader + cbAudioData - 8;

		// NOTE: The "size" field in the RIFF header does not include
		// the first 8 bytes of the file. (That is, the size of the
		// data that appears after the size field.)

		hr = WriteToFile(hFile, &cbRiffFileSize, sizeof(cbRiffFileSize));
	}

	return hr;
}

//-------------------------------------------------------------------
// WriteWaveData
//
// Decodes PCM audio data from the source file and writes it to
// the WAVE file.
//-------------------------------------------------------------------
HRESULT WriteWaveData(HANDLE hFile, IMFSourceReader *pReader, DWORD cbMaxAudioData, DWORD *pcbDataWritten)
{
	HRESULT hr = S_OK;
	DWORD cbAudioData = 0;
	DWORD cbBuffer = 0;
	BYTE *pAudioData = NULL;

	IMFSample *pSample = NULL;
	IMFMediaBuffer *pBuffer = NULL;

	// Get audio samples from the source reader.
	while (true)
	{
		DWORD dwFlags = 0;

		// Read the next sample.
		hr = pReader->ReadSample(
			(DWORD)MF_SOURCE_READER_FIRST_AUDIO_STREAM,
			0, NULL, &dwFlags, NULL, &pSample);

		if (FAILED(hr)) { break; }

		if (dwFlags & MF_SOURCE_READERF_CURRENTMEDIATYPECHANGED || dwFlags & MF_SOURCE_READERF_ENDOFSTREAM)
			break;

		if (pSample == NULL)
			continue;

		// Get a pointer to the audio data in the sample.

		hr = pSample->ConvertToContiguousBuffer(&pBuffer);

		if (FAILED(hr)) { break; }


		hr = pBuffer->Lock(&pAudioData, NULL, &cbBuffer);

		if (FAILED(hr)) { break; }


		// Make sure not to exceed the specified maximum size.
		if (cbMaxAudioData - cbAudioData < cbBuffer)
		{
			cbBuffer = cbMaxAudioData - cbAudioData;
		}

		// Write this data to the output file.
		hr = WriteToFile(hFile, pAudioData, cbBuffer);

		if (FAILED(hr)) { break; }

		// Unlock the buffer.
		hr = pBuffer->Unlock();
		pAudioData = NULL;

		if (FAILED(hr)) { break; }

		// Update running total of audio data.
		cbAudioData += cbBuffer;

		if (cbAudioData >= cbMaxAudioData)
			break;

		SafeRelease(&pSample);
		SafeRelease(&pBuffer);
	}

	if (SUCCEEDED(hr))
		*pcbDataWritten = cbAudioData;

	if (pAudioData)
		pBuffer->Unlock();

	SafeRelease(&pBuffer);
	SafeRelease(&pSample);
	return hr;
}

//-------------------------------------------------------------------
// CalculateMaxAudioDataSize
//
// Calculates how much audio to write to the WAVE file, given the
// audio format and the maximum duration of the WAVE file.
//-------------------------------------------------------------------
DWORD CalculateMaxAudioDataSize(IMFMediaType *pAudioType, DWORD cbHeader, DWORD msecAudioData)
{
	UINT32 cbBlockSize = 0;         // Audio frame size, in bytes.
	UINT32 cbBytesPerSecond = 0;    // Bytes per second.

									// Get the audio block size and number of bytes/second from the audio format.

	cbBlockSize = MFGetAttributeUINT32(pAudioType, MF_MT_AUDIO_BLOCK_ALIGNMENT, 0);
	cbBytesPerSecond = MFGetAttributeUINT32(pAudioType, MF_MT_AUDIO_AVG_BYTES_PER_SECOND, 0);

	// Calculate the maximum amount of audio data to write.
	// This value equals (duration in seconds x bytes/second), but cannot
	// exceed the maximum size of the data chunk in the WAVE file.

	// Size of the desired audio clip in bytes:
	DWORD cbAudioClipSize = (DWORD)MulDiv(cbBytesPerSecond, msecAudioData, 1000);

	// Largest possible size of the data chunk:
	DWORD cbMaxSize = MAXDWORD - cbHeader;

	// Maximum size altogether.
	cbAudioClipSize = min(cbAudioClipSize, cbMaxSize);

	// Round to the audio block size, so that we do not write a partial audio frame.
	cbAudioClipSize = (cbAudioClipSize / cbBlockSize) * cbBlockSize;

	return cbAudioClipSize;
}

//-------------------------------------------------------------------
// WriteWaveHeader
//
// Write the WAVE file header.
//
// Note: This function writes placeholder values for the file size
// and data size, as these values will need to be filled in later.
//-------------------------------------------------------------------
HRESULT WriteWaveHeader(HANDLE hFile, IMFMediaType *pMediaType, DWORD *pcbWritten)
{
	HRESULT hr = S_OK;
	UINT32 cbFormat = 0;

	WAVEFORMATEX *pWav = NULL;

	*pcbWritten = 0;

	// Convert the PCM audio format into a WAVEFORMATEX structure.
	hr = MFCreateWaveFormatExFromMFMediaType(pMediaType, &pWav, &cbFormat);

	// Write the 'RIFF' header and the start of the 'fmt ' chunk.
	if (SUCCEEDED(hr))
	{
		DWORD header[] = {
			// RIFF header
			FCC('RIFF'),
			0,
			FCC('WAVE'),
			// Start of 'fmt ' chunk
			FCC('fmt '),
			cbFormat
		};

		DWORD dataHeader[] = { FCC('data'), 0 };

		hr = WriteToFile(hFile, header, sizeof(header));

		// Write the WAVEFORMATEX structure.
		if (SUCCEEDED(hr))
		{
			hr = WriteToFile(hFile, pWav, cbFormat);
		}

		// Write the start of the 'data' chunk

		if (SUCCEEDED(hr))
		{
			hr = WriteToFile(hFile, dataHeader, sizeof(dataHeader));
		}

		if (SUCCEEDED(hr))
		{
			*pcbWritten = sizeof(header) + cbFormat + sizeof(dataHeader);
		}
	}
	CoTaskMemFree(pWav);
	return hr;
}

//-------------------------------------------------------------------
// ConfigureAudioStream
//
// Selects an audio stream from the source file, and configures the
// stream to deliver decoded PCM audio.
//-------------------------------------------------------------------
HRESULT ConfigureAudioStream(IMFSourceReader *pReader, IMFMediaType **ppPCMAudio)
{
	IMFMediaType *pUncompressedAudioType = NULL;
	IMFMediaType *pPartialType = NULL;

	// Select the first audio stream, and deselect all other streams.
	HRESULT hr = pReader->SetStreamSelection(
		(DWORD)MF_SOURCE_READER_ALL_STREAMS, FALSE);

	if (SUCCEEDED(hr))
	{
		hr = pReader->SetStreamSelection(
			(DWORD)MF_SOURCE_READER_FIRST_AUDIO_STREAM, TRUE);
	}

	// Create a partial media type that specifies uncompressed PCM audio.
	hr = MFCreateMediaType(&pPartialType);

	if (SUCCEEDED(hr))
	{
		hr = pPartialType->SetGUID(MF_MT_MAJOR_TYPE, MFMediaType_Audio);
	}

	if (SUCCEEDED(hr))
	{
		hr = pPartialType->SetGUID(MF_MT_SUBTYPE, MFAudioFormat_PCM);
	}

	// Set this type on the source reader. The source reader will
	// load the necessary decoder.
	if (SUCCEEDED(hr))
	{
		hr = pReader->SetCurrentMediaType(
			(DWORD)MF_SOURCE_READER_FIRST_AUDIO_STREAM,
			NULL, pPartialType);
	}

	// Get the complete uncompressed format.
	if (SUCCEEDED(hr))
	{
		hr = pReader->GetCurrentMediaType(
			(DWORD)MF_SOURCE_READER_FIRST_AUDIO_STREAM,
			&pUncompressedAudioType);
	}

	// Ensure the stream is selected.
	if (SUCCEEDED(hr))
	{
		hr = pReader->SetStreamSelection(
			(DWORD)MF_SOURCE_READER_FIRST_AUDIO_STREAM,
			TRUE);
	}

	// Return the PCM format to the caller.
	if (SUCCEEDED(hr))
	{
		*ppPCMAudio = pUncompressedAudioType;
		(*ppPCMAudio)->AddRef();
	}

	SafeRelease(&pUncompressedAudioType);
	SafeRelease(&pPartialType);
	return hr;
}

//-------------------------------------------------------------------
// WriteWaveFile
//
// Writes a WAVE file by getting audio data from the source reader.
//
//-------------------------------------------------------------------
HRESULT WriteWaveFile(IMFSourceReader *pReader, HANDLE hFile, LONG msecAudioData)
{
	HRESULT hr = S_OK;

	DWORD cbHeader = 0;         // Size of the WAVE file header, in bytes.
	DWORD cbAudioData = 0;      // Total bytes of PCM audio data written to the file.
	DWORD cbMaxAudioData = 0;

	IMFMediaType *pAudioType = NULL;    // Represents the PCM audio format.

										// Configure the source reader to get uncompressed PCM audio from the source file.

	hr = ConfigureAudioStream(pReader, &pAudioType);

	// Write the WAVE file header.
	if (SUCCEEDED(hr))
	{
		hr = WriteWaveHeader(hFile, pAudioType, &cbHeader);
	}

	// Calculate the maximum amount of audio to decode, in bytes.
	if (SUCCEEDED(hr))
	{
		cbMaxAudioData = CalculateMaxAudioDataSize(pAudioType, cbHeader, msecAudioData);

		// Decode audio data to the file.
		hr = WriteWaveData(hFile, pReader, cbMaxAudioData, &cbAudioData);
	}

	// Fix up the RIFF headers with the correct sizes.
	if (SUCCEEDED(hr))
	{
		hr = FixUpChunkSizes(hFile, cbHeader, cbAudioData);
	}

	SafeRelease(&pAudioType);
	return hr;
}

// Convert *.mp3 to *.wav file
PixMp3 bool Convert(WCHAR *wszSourceFile, WCHAR *wszTargetFile)
{
	HeapSetInformation(NULL, HeapEnableTerminationOnCorruption, NULL, 0);
    const unsigned long long MAX_AUDIO_DURATION_MSEC = ULLONG_MAX; // 500 seconds

	HRESULT hr = S_OK;

	IMFSourceReader *pReader = NULL;
	HANDLE hFile = INVALID_HANDLE_VALUE;

	// Initialize the Media Foundation platform.
	if (SUCCEEDED(hr))
	{
		hr = MFStartup(MF_VERSION);
	}

	// Create the source reader to read the input file.
	if (SUCCEEDED(hr))
	{
		hr = MFCreateSourceReaderFromURL(wszSourceFile, NULL, &pReader);
	
		if (FAILED(hr))
			return false;
	}

	// Open the output file for writing.
	if (SUCCEEDED(hr))
	{
		hFile = CreateFile(wszTargetFile, GENERIC_WRITE, FILE_SHARE_READ, NULL,
			CREATE_ALWAYS, 0, NULL);

		if (hFile == INVALID_HANDLE_VALUE)
			return false;
	}

	// Write the WAVE file.
	if (SUCCEEDED(hr))
	{
		hr = WriteWaveFile(pReader, hFile, MAX_AUDIO_DURATION_MSEC);
	}

	if (FAILED(hr))
		return false;

	// Clean up.
	if (hFile != INVALID_HANDLE_VALUE)
	{
		CloseHandle(hFile);
	}

	SafeRelease(&pReader);
	MFShutdown();
	CoUninitialize();

	return SUCCEEDED(hr) ? 0 : 1;
};
