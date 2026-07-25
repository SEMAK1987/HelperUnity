using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// FateContinent Utility: Programmatically fixes bad automatic vertex skin weights from Mixamo.
/// Especially useful when a knight's helmet, collar, or pauldrons stretch unnaturally during head rotation.
/// </summary>
[AddComponentMenu("Fate Tools/Fate Bone Weight Fixer")]
public class FateBoneWeightFixer : MonoBehaviour
{
    [Header("Target Bones Setup")]
    [Tooltip("The head bone in the armature hierarchy (e.g., Mixamorig:Head).")]
    public Transform headBone;
    
    [Tooltip("The neck bone in the armature hierarchy (e.g., Mixamorig:Neck).")]
    public Transform neckBone;

    [Header("Correction Parameters")]
    [Tooltip("Vertices above this height in local space of the mesh will be bound 100% to the Head bone.")]
    public float heightThreshold = 1.6f;

    [Tooltip("If true, removes neck and spine influence from vertices above the threshold.")]
    public bool absoluteHeadBinding = true;

    /// <summary>
    /// Applies skin weight corrections to the SkinnedMeshRenderer's mesh.
    /// </summary>
    [ContextMenu("Fix Head & Helmet Weights")]
    public void FixHeadWeights()
    {
        SkinnedMeshRenderer smr = GetComponent<SkinnedMeshRenderer>();
        if (smr == null)
        {
            Debug.LogError("[Fate Fixer] SkinnedMeshRenderer not found on this GameObject!", this);
            return;
        }

        if (smr.sharedMesh == null)
        {
            Debug.LogError("[Fate Fixer] No sharedMesh assigned to the SkinnedMeshRenderer!", this);
            return;
        }

        // Find the index of the head bone in the SkinnedMeshRenderer's bone list
        int headBoneIndex = -1;
        int neckBoneIndex = -1;

        for (int i = 0; i < smr.bones.Length; i++)
        {
            if (smr.bones[i] == headBone || (headBone == null && smr.bones[i].name.ToLower().Contains("head")))
            {
                headBoneIndex = i;
                if (headBone == null) headBone = smr.bones[i];
            }
            if (smr.bones[i] == neckBone || (neckBone == null && smr.bones[i].name.ToLower().Contains("neck")))
            {
                neckBoneIndex = i;
                if (neckBone == null) neckBone = smr.bones[i];
            }
        }

        if (headBoneIndex == -1)
        {
            Debug.LogError("[Fate Fixer] Head bone could not be identified in the SkinnedMeshRenderer's bone list. Please assign it manually.", this);
            return;
        }

        // Create a copy of the mesh to avoid modifying the original asset on disk directly (unless intended)
        Mesh meshCopy = Instantiate(smr.sharedMesh);
        meshCopy.name = smr.sharedMesh.name + "_WeightsFixed";

        Vector3[] vertices = meshCopy.vertices;
        BoneWeight[] weights = meshCopy.boneWeights;

        if (weights == null || weights.Length == 0)
        {
            Debug.LogError("[Fate Fixer] No bone weights found in the source mesh! Make sure the model is rigged.", this);
            return;
        }

        int correctedCount = 0;

        for (int i = 0; i < vertices.Length; i++)
        {
            // Check the vertical height (Y coordinate) in local space
            float vertexY = vertices[i].y;

            if (vertexY >= heightThreshold)
            {
                BoneWeight bw = weights[i];

                if (absoluteHeadBinding)
                {
                    // Bind completely 100% to the Head Bone
                    bw.boneIndex0 = headBoneIndex;
                    bw.weight0 = 1.0f;
                    bw.weight1 = 0.0f;
                    bw.weight2 = 0.0f;
                    bw.weight3 = 0.0f;
                }
                else
                {
                    // Blend smoothly but prioritize Head Bone
                    if (bw.boneIndex0 != headBoneIndex && bw.boneIndex1 != headBoneIndex &&
                        bw.boneIndex2 != headBoneIndex && bw.boneIndex3 != headBoneIndex)
                    {
                        // Replace the weakest bone weight with Head
                        bw.boneIndex3 = headBoneIndex;
                        bw.weight3 = 0.5f;
                    }
                }

                weights[i] = bw;
                correctedCount++;
            }
        }

        meshCopy.boneWeights = weights;
        
#if UNITY_EDITOR
        // Save as a persistent asset so the fix is not lost when restarting Unity
        string path = AssetDatabase.GetAssetPath(smr.sharedMesh);
        if (!string.IsNullOrEmpty(path))
        {
            string directory = System.IO.Path.GetDirectoryName(path);
            string newPath = System.IO.Path.Combine(directory, meshCopy.name + ".asset");
            AssetDatabase.CreateAsset(meshCopy, newPath);
            AssetDatabase.SaveAssets();
            
            // Re-assign the newly created persistent mesh
            smr.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(newPath);
            Debug.Log($"[Fate Fixer] Programmatic fix complete! Created and saved new mesh asset with fixed weights at: {newPath}", smr);
        }
        else
        {
#endif
            smr.sharedMesh = meshCopy;
            Debug.LogWarning("[Fate Fixer] Fixed mesh in memory only (Runtime style). In Editor, select the original FBX file asset to save it permanently.", smr);
#if UNITY_EDITOR
        }
#endif

        Debug.Log($"[Fate Fixer] Corrected weights for {correctedCount} head/helmet vertices!", this);
    }
}
