#pragma once

typedef __int32 int32_t;
typedef unsigned __int32 uint32_t;


struct H264DecoderResult {
	bool result;			// �f�R�[�h�̐���
	uint32_t w;				// �t���[���̕�
	uint32_t h;				// �t���[���̍���
	uint32_t size;			// �o�b�t�@�T�C�Y
	char*	 buff;			// RGB�f�[�^
};

bool InitH264Decoder();
H264DecoderResult DecodeH264(char* in_data, uint32_t len);
void TermH264Decoder();
char* GetH264DecoderLastError();