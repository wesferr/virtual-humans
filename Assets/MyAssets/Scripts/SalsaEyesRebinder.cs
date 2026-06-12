using System.Collections;
using System.Reflection;
using UMA.CharacterSystem;
using UnityEngine;

public class SalsaEyesRebinder : MonoBehaviour
{
    public DynamicCharacterAvatar avatar;
    public CrazyMinnow.SALSA.Eyes eyes;
    public Transform lookTargetStable;

    void Awake()
    {
        if (!avatar) avatar = GetComponent<DynamicCharacterAvatar>();
        if (!eyes) eyes = GetComponentInChildren<CrazyMinnow.SALSA.Eyes>(true);
    }

    public void RebuildSalsaEyesOnly()
    {
        if (avatar && eyes) StartCoroutine(RebindEyesNextFrame());
    }

    private IEnumerator RebindEyesNextFrame()
    {
        eyes.enabled = false;
        yield return new WaitForEndOfFrame();

        var umaData = avatar.umaData;
        var umaDriver = GetComponent<CrazyMinnow.SALSA.OneClicks.UmaUepDriver>();

        if (umaData?.skeleton != null && umaDriver != null)
        {
            // 1. Interceptador: Deixa o script oficial rodar e ignora o crash esperado nos ossos
            try { umaDriver.CharacterCreated(umaData); } catch { }

            // 2. Busca a verdade absoluta direto no esqueleto do UMA
            Transform head = umaData.animator.GetBoneTransform(HumanBodyBones.Head);
            GameObject lEyeGo = umaData.skeleton.GetBoneGameObject("LeftEye");
            GameObject rEyeGo = umaData.skeleton.GetBoneGameObject("RightEye");

            Transform leftEye = lEyeGo ? lEyeGo.transform : head;
            Transform rightEye = rEyeGo ? rEyeGo.transform : head;

            // 3. Injeta os ossos via Reflexão Segura (evita erros de compilação CS1061)
            TrySetXform(eyes, head, new[] { "Head", "head", "characterHead", "headTransform" });
            TrySetXform(eyes, leftEye, new[] { "LeftEye", "leftEye", "leftEyeTransform", "eyeL" });
            TrySetXform(eyes, rightEye, new[] { "RightEye", "rightEye", "rightEyeTransform", "eyeR" });

            if (lookTargetStable != null)
            {
                TrySetXform(eyes, lookTargetStable, new[] { "LookTarget", "lookTarget", "target" });
            }

            // 4. Força o Initialize do SALSA de forma limpa
            typeof(CrazyMinnow.SALSA.Eyes).GetMethod("Initialize", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)?.Invoke(eyes, null);
        }

        eyes.enabled = true;
    }

    // Trazendo de volta a sua injeção segura
    private bool TrySetXform(object obj, Transform value, string[] candidateNames)
    {
        if (obj == null || value == null) return false;
        var t = obj.GetType();

        foreach (var name in candidateNames)
        {
            var p = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (p != null && p.CanWrite && p.PropertyType == typeof(Transform))
            {
                p.SetValue(obj, value, null);
                return true;
            }
            var f = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f != null && f.FieldType == typeof(Transform))
            {
                f.SetValue(obj, value);
                return true;
            }
        }
        return false;
    }
}