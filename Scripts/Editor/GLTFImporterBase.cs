﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace Siccity.GLTFUtility {
    public abstract class GLTFImporterBase : ScriptedImporter {

        public void SaveToAsset(AssetImportContext ctx, GameObject[] roots) {
#if UNITY_2018_2_OR_NEWER
            // Add GameObjects
            if (roots.Length == 1) {
                ctx.AddObjectToAsset("main", roots[0]);
                ctx.SetMainObject(roots[0]);
            } else {
                GameObject root = new GameObject("Main");
                for (int i = 0; i < roots.Length; i++) {
                    roots[i].transform.parent = root.transform;
                }
                ctx.AddObjectToAsset("main", root);
                ctx.SetMainObject(root);
            }
#else
            // Add GameObjects
            if (roots.Length == 1) {
                ctx.SetMainAsset("main obj", roots[0]);
            } else {
                GameObject root = new GameObject("Main");
                for (int i = 0; i < roots.Length; i++) {
                    roots[i].transform.parent = root.transform;
                }
                ctx.SetMainAsset("main obj", root);
            }
#endif
        }

        public void AddMeshes(AssetImportContext ctx, GLTFObject gltfObject) {
            for (int i = 0; i < gltfObject.meshes.Count; i++) {
                Mesh mesh = gltfObject.meshes[i].GetMesh(gltfObject);
                if (string.IsNullOrEmpty(mesh.name)) mesh.name = i.ToString();

#if UNITY_2018_2_OR_NEWER
                ctx.AddObjectToAsset(gltfObject.meshes[i].name, gltfObject.meshes[i].GetCachedMesh());
#else
                ctx.AddSubAsset(glbObject.meshes[i].name, glbObject.meshes[i].GetCachedMesh());
#endif
            }
        }

        public void AddMaterials(AssetImportContext ctx, GLTFObject gltfObject) {
            for (int i = 0; i < gltfObject.materials.Count; i++) {
                Material mat = gltfObject.materials[i].GetMaterial();
                if (string.IsNullOrEmpty(mat.name)) mat.name = i.ToString();

#if UNITY_2018_2_OR_NEWER
                ctx.AddObjectToAsset(gltfObject.materials[i].name, gltfObject.materials[i].GetMaterial());
#else
                ctx.AddSubAsset(glbObject.materials[i].name, glbObject.materials[i].GetMaterial());
#endif
            }
        }

        public void AddTextures(AssetImportContext ctx, GLTFObject gltfObject) {
            for (int i = 0; i < gltfObject.images.Count; i++) {
                // Dont add asset textures
                if (gltfObject.images[i].imageIsAsset) continue;

                Texture2D tex = gltfObject.images[i].GetTexture();
                if (string.IsNullOrEmpty(tex.name)) tex.name = i.ToString();
#if UNITY_2018_2_OR_NEWER
                ctx.AddObjectToAsset(i.ToString(), tex);
#else
                ctx.AddSubAsset(i.ToString(), glbObject.images[i].GetTexture());
#endif
            }
        }
    }
}