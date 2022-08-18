using Assimp;
using OpenTK;
using OpenTK.Graphics;

namespace XR
{
    public static class Utility
    {
        public static PostProcessSteps GetPostProcessStepsFlags()
        {
            PostProcessSteps postprocess = PostProcessPreset.TargetRealTimeMaximumQuality;

            Settings.Core core = Settings.Core.Default;

            postprocess |= PostProcessSteps.Triangulate;
            //postprocess |= PostProcessSteps.GenerateNormals;
            postprocess |= PostProcessSteps.FlipUVs;

            postprocess = core.ImportGenNormals ?
                postprocess |= PostProcessSteps.GenerateSmoothNormals :
                postprocess &= ~PostProcessSteps.GenerateSmoothNormals;

            postprocess = core.ImportGenTangents ?
                postprocess |= PostProcessSteps.CalculateTangentSpace :
                postprocess &= ~PostProcessSteps.CalculateTangentSpace;

            postprocess = core.ImportOptimize ?
                postprocess |= PostProcessSteps.ImproveCacheLocality :
                postprocess &= ~PostProcessSteps.ImproveCacheLocality;

            postprocess = core.ImportSortByPType ?
                postprocess |= PostProcessSteps.SortByPrimitiveType :
                postprocess &= ~PostProcessSteps.SortByPrimitiveType;

            postprocess = core.ImportRemoveDegenerates ?
                postprocess |= (PostProcessSteps.FindDegenerates | PostProcessSteps.FindInvalidData) :
                postprocess &= ~(PostProcessSteps.FindDegenerates | PostProcessSteps.FindInvalidData);

            postprocess = core.ImportFixInfacing ?
                postprocess |= PostProcessSteps.FixInFacingNormals :
                postprocess &= ~PostProcessSteps.FixInFacingNormals;

            postprocess = core.ImportMergeDuplicates ?
                postprocess |= PostProcessSteps.JoinIdenticalVertices :
                postprocess &= ~PostProcessSteps.JoinIdenticalVertices;

            return postprocess;

        }

        public static Matrix4 FromMatrix4x4(Matrix4x4 matrix4x4) => new Matrix4
        {
            M11 = matrix4x4.A1,
            M12 = matrix4x4.A2,
            M13 = matrix4x4.A3,
            M14 = matrix4x4.A4,
            M21 = matrix4x4.B1,
            M22 = matrix4x4.B2,
            M23 = matrix4x4.B3,
            M24 = matrix4x4.B4,
            M31 = matrix4x4.C1,
            M32 = matrix4x4.C2,
            M33 = matrix4x4.C3,
            M34 = matrix4x4.C4,
            M41 = matrix4x4.D1,
            M42 = matrix4x4.D2,
            M43 = matrix4x4.D3,
            M44 = matrix4x4.D4
        };

        /// <summary>
        ///  Transposed output matrix 
        /// </summary>
        /// <param name="matrix4x4"></param>
        /// <returns></returns>

        public static Matrix4 FromMatrix4x4T(Matrix4x4 matrix4x4) => new Matrix4
        {
            M11 = matrix4x4.A1,
            M12 = matrix4x4.B1,
            M13 = matrix4x4.C1,
            M14 = matrix4x4.D1,

            M21 = matrix4x4.A2,
            M22 = matrix4x4.B2,
            M23 = matrix4x4.C2,
            M24 = matrix4x4.D2,

            M31 = matrix4x4.A3,
            M32 = matrix4x4.B3,
            M33 = matrix4x4.C3,
            M34 = matrix4x4.D3,

            M41 = matrix4x4.A4,
            M42 = matrix4x4.B4,
            M43 = matrix4x4.C4,
            M44 = matrix4x4.D4
        };

        public static OpenTK.Quaternion ConvertQuaternion(Assimp.Quaternion quaternion)
        {
            return new OpenTK.Quaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
        }

        public static Vector3 FromVector3Dto3(Vector3D vector3D) => new Vector3
        {
            X = vector3D.X,
            Y = vector3D.Y,
            Z = vector3D.Z
        };

        public static Vector2 FromVector3Dto2(Vector3D vector3D) => new Vector2
        {
            X = vector3D.X,
            Y = vector3D.Y
        };

        public static Vector2 FromVector2D(Vector2D vector2D) => new Vector2
        {
            X = vector2D.X,
            Y = vector2D.Y
        };

        public static Color4 FromColor4D(Color4D color4D) => new Color4
        {
            R = color4D.R,
            G = color4D.G,
            B = color4D.B,
            A = color4D.A
        };

        public static Vector3 FromColor4DtoVector3(Color4D color4D) => new Vector3
        {
            X = color4D.R,
            Y = color4D.G,
            Z = color4D.B,
        };

        public static string GetSupportedImportFormat()
        {
            return "All Files|*.*|" +
                //COMMON INTERCHANGE FORMATS
                "Autodesk|*.fbx|" +
                "Collada|*.dae|" +
                "glTF|*.gltf, *.glb|" +
                "Blender 3D|*.blend|" +
                "3ds Max 3DS|*.3ds|" +
                "3ds Max ASE|*.ase|" +
                "Wavefront Object|*.obj|" +
                "Industry Foundation Classes|*.ifc|" +
                "XGL|*.xgl, *.zgl|" +
                "Stanford Polygon Library|*.ply|" +
                "AutoCAD DXF|*.dxf|" +
                "LightWave|*.lwo|" +
                "LightWave Scene|*.lws|" +
                "Modo|*.lxo|" +
                "Stereolithography|*.stl|" +
                "DirectX X|*.x|" +
                "AC3D|*.ac|" +
                "Milkshape 3D|*.ms3d|" +
                "TrueSpace|*.cob,.scn|" +
                //MOTION CAPTURE FORMATS
                "Biovision BVH|*.bvh|" +
                "CharacterStudio Motion|*.csm|" +
                //GRAPHICS ENGINE FORMATS
                "Ogre XML|*.xml|" +
                "Irrlicht Mesh|*.irrmesh|" +
                "Irrlicht Scene|*.irr|" +
                //GAME FILE FORMATS
                "Quake I|*.mdl|" +
                "Quake II|*.md2|" +
                "Quake III Mesh|*.md3|" +
                "Quake III Map / BSP|*.pk3|" +
                "Return to Castle Wolfenstein|*.mdc|" +
                "Doom 3|*.md5*|" +
                "Valve Model|*.smd, *.vta|" +
                "Open Game Engine Exchange|*.ogex|" +
                "Unreal|*.3d|" +
                //OTHER FILE FORMATS
                "BlitzBasic 3D|*.b3d|" +
                "Quick3D|*.q3d, *.q3s|" +
                "Neutral File Format|*.nff|" +
                "Sense8 WorldToolKit|*.nff|" +
                "Object File Format|*.off|" +
                "PovRAY Raw|*.raw|" +
                "Terragen Terrain|*.ter|" +
                "3D GameStudio(3DGS)|*.mdl|" +
                "3D GameStudio(3DGS) Terrain|*.hmp|" +
                "Izware Nendo|*.ndo";
        }

        public static string GetSupportedImportFormat2()
        {
            return
            "All Supported Formats|" +
                 "*.fbx; *.dae; *.blend; *.3ds; *.ase; *.obj; *.ifc; *.xgl; *.zgl; *.gltf; *.glb;" +
                "*.ifc; *.ply; *.dxf; *.lwo; *.lws; *.lxo; *.stl; *.x; *.ac; *.ms3d; *.cob; *.scn;" +
                 "*.bvh; *.csm;" +
                 "*.xml; *.irrmesh; *.irr;" +
                 "*.mdl; *.md2; *.md3; *.pk3; *.mdc; *.md5*; *.smd; *.vta; *.ogex; *.3d;" +
                 "*.b3d; *.q3d; *.q3s; *.nff; *.off; *.raw; *.ter; *.mdl; *.hmp; *.ndo" +
            "|Common Interchange Formats|" +
                 "*.fbx; *.dae; *.blend; *.3ds; *.ase; *.obj; *.ifc; *.xgl; *.zgl; *.gltf; *.glb;" +
                 "*.ifc; *.ply; *.dxf; *.lwo; *.lws; *.lxo; *.stl; *.x; *.ac; *.ms3d; *.cob; *.scn" +
             "|Motion Capture Formats|" +
                "*.bvh; *.csm" +
             "|Graphics Engine Formats|" +
                "*.xml; *.irrmesh; *.irr" +
             "|Game File Formats|" +
                "*.mdl; *.md2; *.md3; *.pk3; *.mdc; *.md5*; *.smd; *.vta; *.ogex; *.3d" +
             "|Other File Formats|" +
                "*.b3d; *.q3d; *.q3s; *.nff; *.off; *.raw; *.ter; *.mdl; *.hmp; *.ndo";
        }

        public static Vector4 GetGridColor() => new Vector4()
        {
            X = Settings.Properties.Default.GridColor.R / 255f,
            Y = Settings.Properties.Default.GridColor.G / 255f,
            Z = Settings.Properties.Default.GridColor.B / 255f,
            W = Settings.Properties.Default.GridColor.A / 255f
        };

    }
}
