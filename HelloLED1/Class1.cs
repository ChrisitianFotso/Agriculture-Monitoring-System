using System;

public class FakeSensor
{
    static ushort counter = 0;
    Random rnd = new Random();
	public FakeSensor()
	{
	}
    public short rndTemp()
    {
         rnd.Next(, 65535);
    }
    public ushort rndHumidity()
    {
        
    }

}
