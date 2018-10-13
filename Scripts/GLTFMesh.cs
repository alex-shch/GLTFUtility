using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Siccity.GLTFUtility {
    [Serializable]
    public class GLTFMesh {
        public string name;
        public List<GLTFPrimitive> primitives;
        /// <summary> Morph target weights </summary>
        public List<float> weights;

        private Mesh cache;

        public Mesh GetMesh(GLTFObject gLTFObject) {
            if (cache) return cache;
            else {
                if (primitives.Count == 0) {
                    Debug.LogWarning("0 primitives in mesh");
                    return null;
                } else if (primitives.Count == 1) {
                    Mesh mesh;
                    mesh = new Mesh();

                    // Name
                    mesh.name = name;

                    // Verts
                    if (primitives[0].attributes.POSITION != -1) {
                        mesh.vertices = gLTFObject.accessors[primitives[0].attributes.POSITION].ReadVec3(gLTFObject);
                    }

                    // Tris
                    if (primitives[0].indices != -1) {
                        mesh.triangles = gLTFObject.accessors[primitives[0].indices].ReadInt(gLTFObject);
                    }

                    // Normals
                    if (primitives[0].attributes.NORMAL != -1) {
                        mesh.normals = gLTFObject.accessors[primitives[0].attributes.NORMAL].ReadVec3(gLTFObject);
                    } else mesh.RecalculateNormals();

                    // Tangents
                    if (primitives[0].attributes.TANGENT != -1) {
                        mesh.tangents = gLTFObject.accessors[primitives[0].attributes.TANGENT].ReadVec4(gLTFObject);
                    } else mesh.RecalculateTangents();

                    // Weights
                    if (primitives[0].attributes.WEIGHTS_0 != -1 && primitives[0].attributes.JOINTS_0 != -1) {
                        Vector4[] weights, joints;
                        GetWeights(gLTFObject, out weights, out joints);
                        if (joints.Length == weights.Length) {
                            BoneWeight[] boneWeights = new BoneWeight[weights.Length];
                            for (int i = 0; i < boneWeights.Length; i++) {

                                Vector4 b = weights[i];
                                Vector4 j = joints[i];
                                NormalizeWeights(ref weights[i]);
                                //Debug.Log("After: " + weights[i] + "\\" + joints[i] + "\n" + "Before: " + b + "\\" + j);
                                boneWeights[i].weight0 = weights[i].x;
                                boneWeights[i].weight1 = weights[i].y;
                                boneWeights[i].weight2 = weights[i].z;
                                boneWeights[i].weight0 = weights[i].x;
                                boneWeights[i].boneIndex0 = Mathf.RoundToInt(joints[i].x);
                                boneWeights[i].boneIndex1 = Mathf.RoundToInt(joints[i].y);
                                boneWeights[i].boneIndex2 = Mathf.RoundToInt(joints[i].z);
                                boneWeights[i].boneIndex3 = Mathf.RoundToInt(joints[i].w);
                            }
                            mesh.boneWeights = boneWeights;
                        } else Debug.LogWarning("WEIGHTS_0 and JOINTS_0 not same length. Skipped");
                    }

                    // UVs
                    if (primitives[0].attributes.TEXCOORD_0 != -1) { // UV 1
                        Vector2[] uvs = gLTFObject.accessors[primitives[0].attributes.TEXCOORD_0].ReadVec2(gLTFObject);
                        FlipY(ref uvs);
                        mesh.uv = uvs;
                    }
                    if (primitives[0].attributes.TEXCOORD_1 != -1) { // UV 2
                        Vector2[] uvs = gLTFObject.accessors[primitives[0].attributes.TEXCOORD_1].ReadVec2(gLTFObject);
                        FlipY(ref uvs);
                        mesh.uv2 = uvs;
                    }
                    if (primitives[0].attributes.TEXCOORD_2 != -1) { // UV 3
                        Vector2[] uvs = gLTFObject.accessors[primitives[0].attributes.TEXCOORD_2].ReadVec2(gLTFObject);
                        FlipY(ref uvs);
                        mesh.uv3 = uvs;
                    }
                    if (primitives[0].attributes.TEXCOORD_3 != -1) { // UV 4
                        Vector2[] uvs = gLTFObject.accessors[primitives[0].attributes.TEXCOORD_3].ReadVec2(gLTFObject);
                        FlipY(ref uvs);
                        mesh.uv4 = uvs;
                    }

                    mesh.RecalculateBounds();
                    cache = mesh;
                    return mesh;
                } else {
                    Debug.LogError("Multiple primitives per mesh not supported");
                    return null;
                }
            }
        }

        /// <summary> Get a single vector array sampled from up to 4 vector arrays. Unity only supports 4 bones per vertex, so we look through the weights and pivk the most influential. </summary>
        public void GetWeights(GLTFObject gLTFObject, out Vector4[] weights, out Vector4[] joints) {
            Vector4[] weights0 = (primitives[0].attributes.WEIGHTS_0 != -1) ? gLTFObject.accessors[primitives[0].attributes.WEIGHTS_0].ReadVec4(gLTFObject) : null;
            Vector4[] weights1 = (primitives[0].attributes.WEIGHTS_1 != -1) ? gLTFObject.accessors[primitives[0].attributes.WEIGHTS_1].ReadVec4(gLTFObject) : null;
            Vector4[] weights2 = (primitives[0].attributes.WEIGHTS_2 != -1) ? gLTFObject.accessors[primitives[0].attributes.WEIGHTS_2].ReadVec4(gLTFObject) : null;
            Vector4[] weights3 = (primitives[0].attributes.WEIGHTS_3 != -1) ? gLTFObject.accessors[primitives[0].attributes.WEIGHTS_3].ReadVec4(gLTFObject) : null;
            Vector4[] joints0 = (primitives[0].attributes.JOINTS_0 != -1) ? gLTFObject.accessors[primitives[0].attributes.JOINTS_0].ReadVec4(gLTFObject) : null;
            Vector4[] joints1 = (primitives[0].attributes.JOINTS_1 != -1) ? gLTFObject.accessors[primitives[0].attributes.JOINTS_1].ReadVec4(gLTFObject) : null;
            Vector4[] joints2 = (primitives[0].attributes.JOINTS_2 != -1) ? gLTFObject.accessors[primitives[0].attributes.JOINTS_2].ReadVec4(gLTFObject) : null;
            Vector4[] joints3 = (primitives[0].attributes.JOINTS_3 != -1) ? gLTFObject.accessors[primitives[0].attributes.JOINTS_3].ReadVec4(gLTFObject) : null;
            int bones = 0;
            if (weights0 != null && joints0 != null) bones += 4;
            if (weights1 != null && joints1 != null) bones += 4;
            if (weights2 != null && joints2 != null) bones += 4;
            if (weights3 != null && joints3 != null) bones += 4;
            float[][] allWeights = new float[weights0.Length][];
            float[][] allJoints = new float[weights0.Length][];
            for (int i = 0; i < weights0.Length; i++) {
                allWeights[i] = new float[bones];
                allJoints[i] = new float[bones];
                if (weights0 != null) {
                    allWeights[i][0] = weights0[i].x;
                    allWeights[i][1] = weights0[i].y;
                    allWeights[i][2] = weights0[i].z;
                    allWeights[i][3] = weights0[i].w;
                    allJoints[i][0] = joints0[i].x;
                    allJoints[i][1] = joints0[i].y;
                    allJoints[i][2] = joints0[i].z;
                    allJoints[i][3] = joints0[i].w;
                } else continue;
                if (weights1 != null) {
                    allWeights[i][4] = weights1[i].x;
                    allWeights[i][5] = weights1[i].y;
                    allWeights[i][6] = weights1[i].z;
                    allWeights[i][7] = weights1[i].w;
                    allJoints[i][4] = joints1[i].x;
                    allJoints[i][5] = joints1[i].y;
                    allJoints[i][6] = joints1[i].z;
                    allJoints[i][7] = joints1[i].w;
                } else continue;
                if (weights2 != null) {
                    allWeights[i][8] = weights2[i].x;
                    allWeights[i][9] = weights2[i].y;
                    allWeights[i][10] = weights2[i].z;
                    allWeights[i][11] = weights2[i].w;
                    allJoints[i][8] = joints2[i].x;
                    allJoints[i][9] = joints2[i].y;
                    allJoints[i][10] = joints2[i].z;
                    allJoints[i][11] = joints2[i].w;
                } else continue;
                if (weights3 != null) {
                    allWeights[i][12] = weights3[i].x;
                    allWeights[i][13] = weights3[i].y;
                    allWeights[i][14] = weights3[i].z;
                    allWeights[i][15] = weights3[i].w;
                    allJoints[i][12] = joints3[i].x;
                    allJoints[i][13] = joints3[i].y;
                    allJoints[i][14] = joints3[i].z;
                    allJoints[i][15] = joints3[i].w;
                } else continue;
            }
            weights = new Vector4[weights0.Length];
            joints = new Vector4[joints0.Length];
            for (int i = 0; i < weights0.Length; i++) {
                int x, y, z, w;
                GetHighestIndices(out x, out y, out z, out w, allWeights[i]);

                weights[i] = new Vector4(
                    x >= 0 ? allWeights[i][x] : 0,
                    y >= 0 ? allWeights[i][y] : 0,
                    z >= 0 ? allWeights[i][z] : 0,
                    w >= 0 ? allWeights[i][w] : 0
                );
                joints[i] = new Vector4(
                    x >= 0 ? allJoints[i][x] : 0,
                    y >= 0 ? allJoints[i][y] : 0,
                    z >= 0 ? allJoints[i][z] : 0,
                    w >= 0 ? allJoints[i][w] : 0
                );
                Debug.Log("R: " + weights[i] + "\\" + joints[i] + "\n" +
                    "0: " + new Vector4(allWeights[i][0], allWeights[i][1], allWeights[i][2], allWeights[i][3]) + "\\" + new Vector4(allJoints[i][0], allJoints[i][1], allJoints[i][2], allJoints[i][3]) + "\n" +
                    "1: " + new Vector4(allWeights[i][4], allWeights[i][5], allWeights[i][6], allWeights[i][7]) + "\\" + new Vector4(allJoints[i][4], allJoints[i][5], allJoints[i][6], allJoints[i][7]));
            }
        }

        private void GetHighestIndices(out int x, out int y, out int z, out int w, params float[] weights) {
            // Init indices
            x = -1; // Highest
            y = -1; // Second-highest
            z = -1; // Third-highest
            w = -1; // Fourth-highest

            // Init weight cache
            float xf = 0f, yf = 0f, zf = 0f, wf = 0f;

            for (int i = 0; i < weights.Length; i++) {
                if (weights[i] > xf) {
                    w = z;
                    wf = zf;
                    z = y;
                    zf = yf;
                    y = x;
                    yf = xf;
                    x = i;
                    xf = weights[i];
                    continue;
                } else if (weights[i] > yf) {
                    w = z;
                    wf = zf;
                    z = y;
                    zf = yf;
                    y = i;
                    yf = weights[i];
                    continue;
                } else if (weights[i] > zf) {
                    w = z;
                    wf = zf;
                    z = i;
                    zf = weights[i];
                    continue;
                } else if (weights[i] > wf) {
                    w = i;
                    wf = weights[i];
                    continue;
                }
            }
        }

        public void NormalizeWeights(ref Vector4 weights) {
            float total = weights.x + weights.y + weights.z + weights.w;
            float mult = 1f / total;
            weights.x *= mult;
            weights.y *= mult;
            weights.z *= mult;
            weights.w *= mult;
        }

        public void FlipY(ref Vector2[] uv) {
            for (int i = 0; i < uv.Length; i++) {
                uv[i].y = 1 - uv[i].y;
            }
        }

        public Mesh GetCachedMesh() {
            if (!cache) Debug.LogWarning("No mesh cached for " + name);
            return cache;
        }
    }
}