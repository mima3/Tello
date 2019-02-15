#pragma once
#include <stdexcept>
typedef __int32 int32_t;
typedef unsigned __int32 uint32_t;

struct AVCodecContext;
struct AVFrame;
struct AVCodec;
struct AVCodecParserContext;
struct SwsContext;
struct AVPacket;

typedef unsigned char ubyte;


struct H264DecoderResult {
	int w;				// �t���[���̕�
	int h;				// �t���[���̍���
	int size;			// �o�b�t�@�T�C�Y
	char*	 buff;			// RGB�f�[�^
};


class H264Exception : public std::runtime_error
{
public:
	H264Exception(const char* s) : std::runtime_error(s) {}
};



class H264InitFailure : public H264Exception
{
public:
	H264InitFailure(const char* s) : H264Exception(s) {}
};



class H264DecodeFailure : public H264Exception
{
public:
	H264DecodeFailure(const char* s) : H264Exception(s) {}

};


class H264Decoder
{
	/* Persistent things here, using RAII for cleanup. */
	AVCodecContext        *context;
	AVFrame               *frame;
	AVCodec               *codec;
	AVCodecParserContext  *parser;
	/* In the documentation example on the github master branch, the
	packet is put on the heap. This is done here to store the pointers
	to the encoded data, which must be kept around  between calls to
	parse- and decode frame. In release 11 it is put on the stack, too.
	*/
	AVPacket              *pkt;
public:
	H264Decoder();
	~H264Decoder();
	/* First, parse a continuous data stream, dividing it into
	packets. When there is enough data to form a new frame, decode
	the data and return the frame. parse returns the number
	of consumed bytes of the input stream. It stops consuming
	bytes at frame boundaries.
	*/
	size_t parse(const unsigned char* in_data, size_t in_size);
	bool is_frame_available() const;
	const AVFrame& decode_frame();
};


class ConverterRGB24

{
	SwsContext *context;
	AVFrame *framergb;

public:
	ConverterRGB24();
	~ConverterRGB24();


	/*  Returns, given a width and height,
	how many bytes the frame buffer is going to need. */

	int predict_size(int w, int h);

	/*  Given a decoded frame, convert it to RGB format and fill

	out_rgb with the result. Returns a AVFrame structure holding

	additional information about the RGB frame, such as the number of

	bytes in a row and so on. */

	const AVFrame& convert(const AVFrame &frame, unsigned char* out_rgb);

};