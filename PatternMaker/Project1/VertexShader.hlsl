
struct VS_OUT
{

	float4 pos : SV_POSITION;
};


VS_OUT main(uint vid : SV_VertexID)
{
	VS_OUT output;
	if (vid == 0)
	{
		output.pos = float4(-.5f,.0f, .0f, 1.0f);
		
	}
	if(vid == 1)
	{

		output.pos = float4(.5f, .0f, .0f, 1.0f);

	}
	 if (vid == 2)
	{
		output.pos = float4(.5f, 1.0f, .0f, 1.0f);

	}
	

	return output;
}