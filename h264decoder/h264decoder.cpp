// h264decoder.cpp : DLL アプリケーション用にエクスポートされる関数を定義します。
//

#include "stdafx.h"
#include "h264DecoderImpl.h"
extern "C" {
#include <libavcodec/avcodec.h>
#include <libavutil/avutil.h>
#include <libavutil/mem.h>
#include <libswscale/swscale.h>
}
#include <utility>


#include "h264DecoderImpl.h"
#include <tuple>

// file
#include <iostream>
#include <string>
#include <fstream>



#ifdef __cplusplus
#define DLLEXPORT extern "C" __declspec(dllexport)
#else
#define DLLEXPORT __declspec(dllexport)
#endif


char errormsg[256];
H264Decoder* decoder = NULL;
ConverterRGB24* converter = NULL;

std::pair<int, int> width_height(const AVFrame& f)
{
	return std::make_pair(f.width, f.height);
}



int row_size(const AVFrame& f)
{
	return f.linesize[0];
}





void disable_logging()
{
	av_log_set_level(AV_LOG_QUIET);
}

DLLEXPORT bool InitH264Decoder()
{
	sprintf(errormsg, "");
	try
	{
		decoder = new H264Decoder();
		converter = new ConverterRGB24();
		return true;
	}
	catch (H264InitFailure e)
	{
		sprintf(errormsg, e.what());
		return false;
	}
}

DLLEXPORT void TermH264Decoder()
{
	if (decoder != NULL)
	{
		delete decoder;
		decoder = NULL;
	}

	if (converter != NULL)
	{
		delete converter;
		converter = NULL;
	}
}

DLLEXPORT bool DecodeH264(char* in_data, int len, H264DecoderResult* ret)
{
	sprintf(errormsg, "");

	ret->w = 0;
	ret->h = 0;
	ret->size = 0;
	ret->buff = NULL;
	bool result = false;
	//
	while (len > 0)
	{
		size_t num_consumed = 0;
		bool is_frame_available = false;

		try
		{
			num_consumed = decoder->parse((ubyte*)in_data, len);
			if (is_frame_available = decoder->is_frame_available())
			{
				const auto frame = decoder->decode_frame();
				int w, h; std::tie(w, h) = width_height(frame);
				size_t out_size = converter->predict_size(w, h);
				char* out_buff = (char*)malloc(out_size);
				memset(out_buff, 0x00, out_size);
				const auto &rgbframe = converter->convert(frame, (ubyte*)out_buff);

				ret->buff = out_buff;
				ret->size = out_size;
				ret->w = w;
				ret->h = h;
				result = true;

			}

		}
		catch (const H264DecodeFailure &e)
		{
			if (num_consumed <= 0)
			{
				// This case is fatal because we cannot continue to move ahead in the stream.
				sprintf(errormsg, e.what());
				return result;
			}
		}
		len -= num_consumed;
		in_data += num_consumed;

	}
	return result;

}

DLLEXPORT char*  GetH264DecoderLastError()
{
	return &errormsg[0];
}

DLLEXPORT void FreeData(char* data)
{
	free(data);
}