#if UNITY_EDITOR
// use case: you have a separate roughness / metalic map and you want to add them to a relevant texture's
// alpha channel

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace K3.Editor { 

    [UnityEngine.Scripting.Preserve]
    public static class MapCombiner {
        [MenuItem("Assets/K3 - Create material from textures")]
        public static void TryCombineTextures() {
            _DoTryCombineTextures(true);
        }

        static void _DoTryCombineTextures(bool allowConcatenation) {
            var anal = PerformSelectionAnalysis();
            if (allowConcatenation) { 
                (var didConcatenate, var concatenatedPath) = ConcatenateTexturesWherePossible(anal);
                if (didConcatenate) {
                    AssetDatabase.Refresh();
                    Debug.LogWarning($"Did not create the material yet; I had to generate a combined smothness / metallic map");
                    IncludeObjectAtPathToSelection(concatenatedPath);
                    _DoTryCombineTextures(false);
                    return;
                }
            }
            
            TryGenerateMaterial(anal);
        }

        private static void IncludeObjectAtPathToSelection(string concatenatedPath) {
            var objects = Selection.objects.ToList();
            
            var filePath = GetPaths(concatenatedPath).relativeFilePath;
            var asset = AssetDatabase.LoadAssetAtPath<Texture2D>(filePath);
            objects.Add(asset);
            Selection.objects = objects.ToArray();
        }

        [MenuItem("Assets/K3 - Create material from textures", true)]
        public static bool IsValidMaterialCombine() {
            var anal = PerformSelectionAnalysis();
            if (anal.countOfKnownTextureUsages > 0) {
                return true;
            }
            return false;
        }


        private static ObjectAnalysis PerformSelectionAnalysis() {
            var selectedItems = AnalyzeSelection();
            var objAnal = new ObjectAnalysis() {
                items = selectedItems
            };
            objAnal.PruneTexturesOnly();
            if (objAnal.items.Length == 0) return objAnal;

            var prefix = objAnal.CalculatePrefix();
            if (prefix.Length > 2) {
                objAnal.InferUsages();
            }
            return objAnal;
        }

        class ObjectAnalysis {
            internal AnalyzedItem[] items;
            internal int countOfUnknownTextureUsages;
            internal int countOfKnownTextureUsages;
            internal string commonPrefix;

            internal void PruneTexturesOnly() {
                items = items.Where(item => item.type == ObjectAnalysisType.Texture).ToArray();
            }

            internal string CalculatePrefix() {
                var names = GetNames(items.Select(a => a.texture));
                var prefix = FindCommonPrefix(names);
                if (prefix.Length > 2) {
                    for (var i = 0; i < items.Length; i++) {
                        items[i].specificIdentifier = items[i].texture.name.Substring(prefix.Length);
                    }
                }
                this.commonPrefix = prefix;
                return prefix;
            }

            internal void InferUsages() {
                for (var i = 0; i < items.Length; i++) {
                    items[i].usage = InferTextureUsage(items[i].specificIdentifier);
                    if (items[i].usage == TextureUsages.Unknown) {
                        Debug.Log($"Unknown texture postfix: `{items[i].specificIdentifier}`");
                        this.countOfUnknownTextureUsages++;
                    } else {
                        this.countOfKnownTextureUsages++;
                    }
                }
            }
        
        }

        static AnalyzedItem[] AnalyzeSelection() {
            List<AnalyzedItem> result = new();
            var objs = Selection.objects;
            var iids = Selection.instanceIDs;
            for (var i = 0; i < iids.Length; i++) {
                var anal = new AnalyzedItem();
                var iid = iids[i];
                var obj = objs[i];

                anal.path = AssetDatabase.GetAssetPath(iid);

                if (obj is Texture2D tex) {
                    anal.type = ObjectAnalysisType.Texture;
                    anal.texture = tex;
                } else {
                    anal.type = ObjectAnalysisType.Other;
                }

                result.Add(anal);
            }
            return result.ToArray();
        }

        enum ObjectAnalysisType {
            Texture,
            Other,
        }

        struct AnalyzedItem {
            internal ObjectAnalysisType type;
            internal Texture2D texture;
            internal TextureUsages usage;
            internal string specificIdentifier;
            internal string commonPath;
            internal string path;
        }

        enum TextureUsages {
            Unknown,
            Albedo,
            Normal,
            Roughness,
            Smoothness,
            MetallicRaw,
            Emission,
            Height,
            Occlusion,
            CombinedMetallicSmoothness,
            Other
        }

        static Dictionary<string, TextureUsages> usageMap;
        
        static TextureUsages InferTextureUsage(string str) {
            if (usageMap == null) GenerateUsageMap();
            if (!usageMap.TryGetValue(str, out var val)) return TextureUsages.Unknown;
            return val;
        }

        private static void GenerateUsageMap() {
            usageMap = new Dictionary<string, TextureUsages> {
                { "albedo", TextureUsages.Albedo },
                { "diffuse", TextureUsages.Albedo },
                { "basecolor", TextureUsages.Albedo },
                { "normal", TextureUsages.Normal },
                { "roughness", TextureUsages.Roughness },
                { "smoothness", TextureUsages.Smoothness },
                { "glossiness", TextureUsages.Smoothness },
                { "metallic", TextureUsages.MetallicRaw },
                { "emission", TextureUsages.Emission },
                { "emissive", TextureUsages.Emission },
                { "height", TextureUsages.Height },
                { "occlusion", TextureUsages.Occlusion },
                { "ambientocclusion", TextureUsages.Occlusion },
                { "metallicsmoothness", TextureUsages.CombinedMetallicSmoothness }
            };

            foreach (var key in usageMap.Keys.ToArray()) {
                if (char.IsLower(key[0])) {
                    var newKey = char.ToUpperInvariant(key[0]).ToString() + key.Substring(1);
                    usageMap[newKey] = usageMap[key];
                }
            }
        }

        private static void TryGenerateMaterial(ObjectAnalysis anal) {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            var mat = new Material(shader);
        
            foreach (var item in anal.items) {
                if (item.usage != TextureUsages.Unknown) {
                    Debug.Log($"Assigning texture {item.texture.name} as {item.usage}");
                    AssignTexture(item.usage, mat, item.texture);
                }
            }

            var filepathInfo = GetPaths(anal.items[0].path);
            var materialPath = System.IO.Path.Combine(filepathInfo.relativeFileDirectoryPath, $"{GetMaterialName(anal)}.mat");
            
            AssetDatabase.CreateAsset(mat, materialPath); 
            AssetDatabase.SaveAssets();    
        }

        static (string relativeFilePath, string relativeFileDirectoryPath, string absoluteFileDirectoryPath) GetPaths(string assetPath) {
            var projectRoot = Paths.Editor.GetPathInUnityProject();
            var path = new System.IO.FileInfo(assetPath);
            var directory = path.Directory;

            if (!path.FullName.StartsWith(projectRoot)) throw new InvalidOperationException($"Asset path {assetPath} does not start with project root {projectRoot} ");
            return (
                path.FullName.Substring(projectRoot.Length + 1), 
                directory.FullName.Substring(projectRoot.Length + 1),
                path.Directory.FullName
            );
        }

        private static string GetMaterialName(ObjectAnalysis anal) {
            return anal.commonPrefix.TrimEnd(new[] { '_', '-', ' ' });
        }

        static void AssignTexture(TextureUsages usage, Material mat, Texture2D texture) {


            var isUrp = UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline;
            var str = usage switch {
                TextureUsages.Albedo => isUrp ? "_BaseMap" : "_MainTex",            
                TextureUsages.Normal => "_BumpMap",
                TextureUsages.Emission => "_EmissionMap",
                TextureUsages.Height => "_ParallaxMap",
                TextureUsages.Occlusion => "_OcclusionMap", 
                TextureUsages.CombinedMetallicSmoothness => "_MetallicGlossMap", 
                _ => null
            };

            if (str == null) return;
            
            if (!mat.HasTexture(str)) 
                Debug.LogError($"Material {mat} doesn't have a property for {str}");
            else 
                mat.SetTexture(str, texture);
        }

    
        //static Texture2D LoadTexture(string fullPath) {
        //    Texture2D myTexture = new Texture2D( 1, 1 );
        //    myTexture.LoadImage( System.IO.File.ReadAllBytes(fullPath) );
        //    myTexture.Apply();
        //    return myTexture;
        //}
    

        static IEnumerable<string> GetNames(IEnumerable<UnityEngine.Object> objects) => objects.Select(o => o.name);
    
        static string FindCommonPrefix(IEnumerable<string> samples) {
        
            var commonPrefix = new string(
                samples.First().Substring(0, samples.Min(s => s.Length))
                .TakeWhile((c, i) => samples.All(s => s[i] == c)).ToArray());
            return commonPrefix;
        }

    
        private static (bool did, string concatenatedPath) ConcatenateTexturesWherePossible(ObjectAnalysis anal) {
            // if roughness exists, but Smoothness does not, generate smoothness in memory (!) and assign to obj analysis
            var roughnessExists = anal.items.Any(item => item.usage == TextureUsages.Roughness);
            var smoothnessExists = anal.items.Any(item => item.usage == TextureUsages.Smoothness);
            var metallicExists = anal.items.Any(item => item.usage == TextureUsages.MetallicRaw);

            var msexists = anal.items.Any(item => item.usage == TextureUsages.CombinedMetallicSmoothness);
            if (msexists) return default;

            if (smoothnessExists && metallicExists) {
                var metallicItem = anal.items.First(item => item.usage == TextureUsages.MetallicRaw);
                var smoothItem   = anal.items.First(item => item.usage == TextureUsages.Smoothness);
                var metallicItemIndex = System.Array.IndexOf(anal.items, metallicItem);
                // anal.items[metallicItemIndex].texture = GenerateCombinedSmoothnessAndGlossTexture(anal, metallicItem, smoothItem);
                return (true, GenerateCombinedSmoothnessAndGlossTexture(anal, metallicItem, smoothItem));
            }

            else if (roughnessExists && metallicExists) {
                var metallicItem = anal.items.First(item => item.usage == TextureUsages.MetallicRaw);
                var roughItem   = anal.items.First(item => item.usage == TextureUsages.Roughness);
                var metallicItemIndex = System.Array.IndexOf(anal.items, metallicItem);
                //anal.items[metallicItemIndex].texture = GenerateCombinedSmoothnessAndGlossTexture(anal, metallicItem, roughItem);
                return (true, GenerateCombinedSmoothnessAndGlossTexture(anal, metallicItem, roughItem));
            }
            return default;
        
        }

        private static string GenerateCombinedSmoothnessAndGlossTexture(ObjectAnalysis analysis, AnalyzedItem metallicItem, AnalyzedItem otherItem) {
            var invertValue = (otherItem.usage == TextureUsages.Roughness);

            var combinedTexturePath = System.IO.Path.Combine(GetPaths(metallicItem.path).absoluteFileDirectoryPath, $"{analysis.commonPrefix}metallicsmoothness.png");
            var pngMetallic = LoadPNG(metallicItem.path);
            var pngOther = LoadPNG(otherItem.path);
            var w = pngMetallic.width; 
            var h = pngMetallic.height;

            var combinedTexture = new Texture2D(w, h);

            //for (var ix = 0; ix < w; ix++) {
            //    for (var iy = 0; iy < h; iy++ ) {
            //        var m = pngMetallic.GetPixel(ix, iy).r;
            //        var o = pngOther.GetPixel(ix, iy).r;
            //        var c = new Color(m,m,m,o);
            //        combinedTexture.SetPixel(ix, iy, c);
            //    }
            //}

            var pm = pngMetallic.GetPixels();
            var po = pngOther.GetPixels();
            var resultingPixels = combinedTexture.GetPixels();

            for (var ix = 0; ix < w; ix++) {
                for (var iy = 0; iy < h; iy++ ) {
                    var i = iy * w + ix;
                    var m = pm[i].r;
                    var o = po[i].r;
                    if (invertValue) o = 1f - o;
                    resultingPixels[i] = new Color(m,m,m,o);
                }
            }
            combinedTexture.SetPixels(resultingPixels);

            var combinedBytes = combinedTexture.EncodeToPNG();
            System.IO.File.WriteAllBytes(combinedTexturePath, combinedBytes);

            return combinedTexturePath;
            

            //var pixelsMetallic = LoadPNG(metallicItem.path).GetPixels();
            //var pixelsOther = LoadPNG(otherItem.path).GetPixels();

            ////var pixelsMetallic = metallicItem.texture.GetPixels();
            ////var pixelsOther = otherItem.texture.GetPixels();

            //var w = metallicItem.texture.width;
            //var h = metallicItem.texture.height;

            //if (w != otherItem.texture.width || h != otherItem.texture.height) throw new System.InvalidOperationException($"Smoothness/roughness and metallic texture must have exact same dimensions");

            //Debug.LogWarning($"Generating combined texture : {w}x{h} px");

            //var resultingTexture = new Texture2D(metallicItem.texture.width, metallicItem.texture.height);
            //var pixels = new Color[w * h * 4];

            //for (var x = 0; x < w; x++) {
            //    for (var y = 0; y < h; y++) {
            //        var i = y * w + x;
            //        var metallic = pixelsMetallic[i].r;
            //        var smooth = pixelsOther[i].r;
            //        if (invertValue) smooth = 1f - smooth;
            //        var color = new Color(metallic, metallic, metallic, smooth);
                    
            //        // resultingTexture[i].SetPixel(x, y, color, 0);
            //    }
            //}
            //resultingTexture.SetPixels(pixels);

            //// resultingTexture.SetPixels(pixels);
            //resultingTexture.Apply();

            //var bytes = resultingTexture.EncodeToPNG();
            //var fullPath = System.IO.Path.Combine(GetPaths(metallicItem.path).absoluteFileDirectoryPath, $"{analysis.commonPrefix}-COMBINED_metallic_gloss.png");
            //System.IO.File.WriteAllBytes(fullPath, bytes);

            //var p = GetPaths(fullPath);

            //resultingTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(p.relativeFilePath);

            //metallicItem.texture = resultingTexture;
            //metallicItem.path = p.relativeFilePath;

            //return resultingTexture;
        }

        //static Texture2D CopyUnreadableTexture(Texture2D originalTexture) {
        //    RenderTexture tmp = RenderTexture.GetTemporary( 
        //                        originalTexture.width,
        //                        originalTexture.height,
        //                        0,
        //                        RenderTextureFormat.ARGB32,
        //                        RenderTextureReadWrite.Linear);


        //    // Blit the pixels on texture to the RenderTexture
        //    Graphics.Blit(originalTexture, tmp);


        //    // Backup the currently set RenderTexture
        //    RenderTexture previous = RenderTexture.active;


        //    // Set the current RenderTexture to the temporary one we created
        //    RenderTexture.active = tmp;


        //    // Create a new readable Texture2D to copy the pixels to it
        //    Texture2D readableCopy = new Texture2D(originalTexture.width, originalTexture.height);


        //    // Copy the pixels from the RenderTexture to the new Texture
        //    readableCopy.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
        //    readableCopy.Apply();


        //    // Reset the active RenderTexture
        //    RenderTexture.active = previous;


        //    // Release the temporary RenderTexture
        //    RenderTexture.ReleaseTemporary(tmp);

        //    return readableCopy;
        //    // "myTexture2D" now has the same pixels from "texture" and it's re
        //}

         static Texture2D LoadPNG(string filePath) {
 
             Texture2D tex = null;
             byte[] fileData;
 
             if (File.Exists(filePath))     {
                 fileData = File.ReadAllBytes(filePath);
                 tex = new Texture2D(2, 2);
                 tex.LoadImage(fileData, false); //..this will auto-resize the texture dimensions.
             }
             return tex;
         }

    }

}
#endif