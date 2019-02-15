#pragma once

typedef __int32 int32_t;
typedef unsigned __int32 uint32_t;


struct H264DecoderResult {
	bool result;			// デコードの成否
	uint32_t w;				// フレームの幅
	uint32_t h;				// フレームの高さ
	uint32_t size;			// バッファサイズ
	char*	 buff;			// RGBデータ
};

bool InitH264Decoder();
H264DecoderResult DecodeH264(char* in_data, uint32_t len);
void TermH264Decoder();
char* GetH264DecoderLastError();