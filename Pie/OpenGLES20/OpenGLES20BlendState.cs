using System;
using Silk.NET.OpenGLES;
using static Pie.OpenGLES20.OpenGLES20GraphicsDevice;

namespace Pie.OpenGLES20;

internal sealed class OpenGLES20BlendState : BlendState
{
    private BlendingFactor _src;
    private BlendingFactor _dst;
    
    public OpenGLES20BlendState(BlendStateDescription description)
    {
        _src = GetBlendingFactorFromBlendType(description.Source);
        _dst = GetBlendingFactorFromBlendType(description.Destination);
        Description = description;
    }
    
    public override bool IsDisposed { get; protected set; }
    
    public override BlendStateDescription Description { get; }

    public void Set()
    {
        Gl.Enable(EnableCap.Blend);
        Gl.BlendFunc(_src, _dst);
    }
    
    public override void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;
        // There is nothing to dispose of
    }

    private static BlendingFactor GetBlendingFactorFromBlendType(BlendType type)
    {
        return type switch
        {
            BlendType.Zero => BlendingFactor.Zero,
            BlendType.One => BlendingFactor.One,
            BlendType.SrcColor => BlendingFactor.SrcColor,
            BlendType.OneMinusSrcColor => BlendingFactor.OneMinusSrcColor,
            BlendType.DestColor => BlendingFactor.DstColor,
            BlendType.OneMinusDestColor => BlendingFactor.OneMinusDstColor,
            BlendType.SrcAlpha => BlendingFactor.SrcAlpha,
            BlendType.OneMinusSrcAlpha => BlendingFactor.OneMinusSrcAlpha,
            BlendType.DestAlpha => BlendingFactor.DstAlpha,
            BlendType.OneMinusDestAlpha => BlendingFactor.OneMinusDstAlpha,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}