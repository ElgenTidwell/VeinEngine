struct PSInput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR;
	float2 TexCoord : TEXCOORD;
};

float4 main(PSInput pin) : SV_TARGET
{
	return pin.Color*float4(pin.TexCoord.x,pin.TexCoord.y,0,1);
}