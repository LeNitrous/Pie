﻿using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Spirzza.Interop.Shaderc;
using Spirzza.Interop.SpirvCross;
using static Spirzza.Interop.Shaderc.Shaderc;
using static Spirzza.Interop.SpirvCross.SpirvCross;

namespace Pie.ShaderCompiler;

public static class Compiler
{
    public static unsafe CompilerResult ToSpirv(Stage stage, Language language, byte[] source, string entryPoint, bool reflect = false)
    {
        shaderc_compiler* compiler = shaderc_compiler_initialize();
        shaderc_compile_options* options = shaderc_compile_options_initialize();
        shaderc_compilation_result* result;

        shaderc_source_language sourceLanguage = language switch
        {
            Language.GLSL => shaderc_source_language.shaderc_source_language_glsl,
            Language.HLSL => shaderc_source_language.shaderc_source_language_hlsl,
            _ => throw new ArgumentOutOfRangeException(nameof(language), language, null)
        };

        shaderc_compile_options_set_source_language(options, sourceLanguage);

        shaderc_shader_kind kind = stage switch
        {
            Stage.Vertex => shaderc_shader_kind.shaderc_vertex_shader,
            Stage.Fragment => shaderc_shader_kind.shaderc_fragment_shader,
            Stage.Geometry => shaderc_shader_kind.shaderc_geometry_shader,
            Stage.Compute => shaderc_shader_kind.shaderc_compute_shader,
            _ => throw new ArgumentOutOfRangeException(nameof(stage), stage, null)
        };

        fixed (byte* src = source)
        fixed (sbyte* fn = GetFromString("main"))
        fixed (sbyte* entpt = GetFromString(entryPoint))
        {
            result = shaderc_compile_into_spv(compiler, (sbyte*) src, (nuint) source.Length, kind, fn, entpt, options);
        }

        if (shaderc_result_get_compilation_status(result) !=
            shaderc_compilation_status.shaderc_compilation_status_success)
        {
            string error = ConvertToString(shaderc_result_get_error_message(result));
            
            shaderc_result_release(result);
            shaderc_compiler_release(compiler);

            return new CompilerResult(null, false, $"Failed to convert {stage.ToString().ToLower()} shader: " + error,
                null);
        }

        sbyte* bytes = shaderc_result_get_bytes(result);
        nuint length = shaderc_result_get_length(result);
        
        byte[] compiled = new byte[length];
        
        fixed (byte* cmpPtr = compiled)
            Unsafe.CopyBlock(cmpPtr, bytes, (uint) length);
        
        ReflectionInfo? info = null;
        if (reflect)
            info = ReflectionInfo.FromJson(Encoding.UTF8.GetString(SpirvToShaderCode(spvc_backend.SPVC_BACKEND_JSON, (byte*) bytes, length).Result), stage);
        
        shaderc_result_release(result);
        shaderc_compiler_release(compiler);

        return new CompilerResult(compiled, true, string.Empty, info);
    }

    private static sbyte[] GetFromString(string text)
    {
        return (sbyte[]) (Array) Encoding.ASCII.GetBytes(text);
    }

    private static unsafe string ConvertToString(sbyte* text)
    {
        return Marshal.PtrToStringAnsi((IntPtr) text);
    }

    private static unsafe CompilerResult SpirvToShaderCode(spvc_backend backend, byte* result, nuint length)
    {
        spvc_context* context;
        spvc_context_create(&context);

        spvc_parsed_ir* ir;
        spvc_context_parse_spirv(context, (SpvId*) result,
            (nuint) ((int) length / sizeof(SpvId)), &ir);
        
        spvc_compiler* compl;
        spvc_context_create_compiler(context, backend, ir,
            spvc_capture_mode.SPVC_CAPTURE_MODE_TAKE_OWNERSHIP, &compl);

        spvc_compiler_options* options;
        spvc_compiler_create_compiler_options(compl, &options);
        switch (backend)
        {
            case spvc_backend.SPVC_BACKEND_GLSL:
                spvc_compiler_options_set_uint(options, spvc_compiler_option.SPVC_COMPILER_OPTION_GLSL_VERSION, 430);
                spvc_compiler_options_set_bool(options, spvc_compiler_option.SPVC_COMPILER_OPTION_GLSL_ES, SPVC_FALSE);
                break;
            case spvc_backend.SPVC_BACKEND_HLSL:
                spvc_compiler_options_set_uint(options, spvc_compiler_option.SPVC_COMPILER_OPTION_HLSL_SHADER_MODEL,
                    50);
                spvc_compiler_options_set_bool(options,
                    spvc_compiler_option.SPVC_COMPILER_OPTION_HLSL_FLATTEN_MATRIX_VERTEX_INPUT_SEMANTICS, SPVC_TRUE);
                 break;
            case spvc_backend.SPVC_BACKEND_JSON:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(backend), backend, null);
        }
        spvc_compiler_install_compiler_options(compl, options);

        sbyte* compiledResult;
        spvc_result spirvResult = spvc_compiler_compile(compl, &compiledResult);

        if (spirvResult != spvc_result.SPVC_SUCCESS)
        {
            string error = ConvertToString(spvc_context_get_last_error_string(context));
            spvc_context_destroy(context);

            return new CompilerResult(null, false, error, null);
        }
        
        byte[] compiled = Encoding.UTF8.GetBytes(ConvertToString(compiledResult));
        
        // TODO: Free compiled result??
        spvc_context_destroy(context);

        // TODO: Check spirv result.
        return new CompilerResult(compiled, true, string.Empty, null);
    }

    public static unsafe CompilerResult FromSpirv(Language language, byte[] spirv)
    {
        CompilerResult result;

        spvc_backend backend = language switch
        {
            Language.GLSL => spvc_backend.SPVC_BACKEND_GLSL,
            Language.HLSL => spvc_backend.SPVC_BACKEND_HLSL,
            _ => throw new ArgumentOutOfRangeException(nameof(language), language, null)
        };

        fixed (byte* sPtr = spirv)
            result = SpirvToShaderCode(backend, sPtr, (nuint) spirv.Length);

        return result;
    }
}