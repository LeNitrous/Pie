using System;
using System.Text;
using Pie.Text.Native;
using static Pie.Text.Native.FreetypeNative;

namespace Pie.Text;

public class FreeType : IDisposable
{
    private FT_Library _library;
    
    public FreeType()
    {
        if (FT_Init_FreeType(out _library) != FT_Error.Ok)
            throw new Exception("Could not initialize freetype.");
    }

    public unsafe Face CreateFace(string path, int initialSize, FaceFlags flags = FaceFlags.Antialiased | FaceFlags.RgbaConvert)
    {
        //return CreateFace(File.ReadAllBytes(path), initialSize);
        FT_Face* face;
        fixed (byte* bytes = Encoding.ASCII.GetBytes(path))
            FT_New_Face(_library, (sbyte*) bytes, new FT_Long(0), out face);
        return new Face(face, initialSize, flags);
    }

    public unsafe Face CreateFace(byte[] data, int initialSize, FaceFlags flags = FaceFlags.Antialiased | FaceFlags.RgbaConvert)
    {
        FT_Face* face;
        fixed (byte* d = data)
            FT_New_Memory_Face(_library, d, new FT_Long(data.Length), new FT_Long(0), out face);
        return new Face(face, initialSize, flags);
    }

    public void Dispose()
    {
        if (FT_Done_FreeType(_library) != FT_Error.Ok)
            throw new Exception("An error occured during disposal.");
    }
}