using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using CubismFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using osuTK.Graphics.OpenGL;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using osuTK;
using Avalonia;

namespace TestAvalonia
{
    public class Live2DViewer : OpenGlControlBase
    {
        CubismAsset Asset;
        CubismRenderingManager RenderingManager;
        CubismOpenTKRenderer Renderer;
        CubismMotionQueueEntry LastMotion;
        double elapsed;
        private DebugProc openGLDebugDelegate;

        public Live2DViewer():base()
        {
            
        }

        private static void openGLDebugCallback(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
        {
            Debug.WriteLine(source == DebugSource.DebugSourceApplication ?
                $"{Marshal.PtrToStringAnsi(message, length)}" :
                $"{Marshal.PtrToStringAnsi(message, length)}\n\tid:{id} severity:{severity} type:{type} source:{source}\n");
        }

        protected override void OnOpenGlInit(GlInterface gl, int fb)
        {
            base.OnOpenGlInit(gl, fb);

            Asset = new CubismAsset(@"hiyori_free_t06.model3.json", (string file_path) =>
            {
                string resource_name = Path.GetFileNameWithoutExtension(file_path).Replace('.', '_');
                byte[] byte_array = (byte[])Hiyori.ResourceManager.GetObject(resource_name);
                return new MemoryStream(byte_array);
            });

            var eye_blink_controller = new CubismEyeBlink(Asset.ParameterGroups["EyeBlink"]);
            Asset.StartMotion(MotionType.Effect, eye_blink_controller);

            Renderer = new CubismOpenTKRenderer();
            RenderingManager = new CubismRenderingManager(Renderer, Asset);

            GL.Enable((EnableCap)All.DebugOutput);
            GL.Enable((EnableCap)All.DebugOutputSynchronous);

            openGLDebugDelegate = new DebugProc(openGLDebugCallback);

            GL.DebugMessageCallback(openGLDebugDelegate, IntPtr.Zero);
            GL.DebugMessageControl(DebugSourceControl.DontCare, DebugTypeControl.DontCare, DebugSeverityControl.DontCare, 0, new int[0], true);

            GL.DebugMessageInsert(DebugSourceExternal.DebugSourceApplication, DebugType.DebugTypeMarker, 0, DebugSeverity.DebugSeverityNotification, -1, "Debug output enabled");

        }

        protected override void OnOpenGlRender(GlInterface gl, int fb)
        {
            GL.ClearColor(0.0f, 0.5f, 0.5f, 1.0f);

            if ((LastMotion == null) || (LastMotion.Finished == true))
            {
                var motion_group = Asset.MotionGroups[""];
                int number = new Random().Next() % motion_group.Length;
                var motion = (CubismMotion)motion_group[number];
                LastMotion = Asset.StartMotion(MotionType.Base, motion, false);
            }

            Asset.Update(1.0f/60);

            GL.Viewport(0, 0, (int)Width, (int)Height);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            Matrix4 mvp_matrix = Matrix4.Identity;
            mvp_matrix[0, 0] = 2.0f;
            mvp_matrix[1, 1] = (float)(2.0f * Width / Height);
            RenderingManager.Draw(mvp_matrix);
        }
    }
}
