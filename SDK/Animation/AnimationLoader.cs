﻿using System;
using System.Collections;
using UnityEngine;

namespace BeatSaberPlus.SDK.Animation
{
    public enum AnimationType
    {
        GIF, APNG
    }

    public class AnimationLoader
    {
        public static void Process(AnimationType p_Type, byte[] p_Data, Action<Texture2D, Rect[], float[], int, int> callback)
        {
            switch (p_Type)
            {
                case AnimationType.GIF:
                    GIFDecoder.Process(p_Data,
                        (AnimationInfo animationInfo) => SharedCoroutineStarter.instance.StartCoroutine(ProcessAnimationInfo(animationInfo, callback)));
                    break;
                case AnimationType.APNG:
                    APNGUnityDecoder.Process(p_Data,
                        (AnimationInfo animationInfo) => SharedCoroutineStarter.instance.StartCoroutine(ProcessAnimationInfo(animationInfo, callback)));
                    break;
            }
        }

        public static IEnumerator ProcessAnimationInfo(AnimationInfo p_AnimationInfo, Action<Texture2D, Rect[], float[], int, int> callback)
        {
            Texture2D   l_AtlasTexture  = null;
            Texture2D[] l_SubTextures   = new Texture2D[p_AnimationInfo.frameCount];

            var l_Delays                = new float[p_AnimationInfo.frameCount];
            var l_MaxAtlasTextureSize   = 2048;
            int width = 0;
            int height = 0;

            for (var l_FrameI = 0; l_FrameI < p_AnimationInfo.frameCount; l_FrameI++)
            {
                if (p_AnimationInfo.frames.Count <= l_FrameI)
                {
                    yield return new WaitUntil(() => { return p_AnimationInfo.frames.Count > l_FrameI; });
                }

                if (l_AtlasTexture == null)
                {
                    l_MaxAtlasTextureSize = GetMaxAtlasTextureSize(p_AnimationInfo, l_FrameI);
                    l_AtlasTexture = new Texture2D(p_AnimationInfo.frames[l_FrameI].width, p_AnimationInfo.frames[l_FrameI].height);
                }

                var l_CurrentFrameInfo = p_AnimationInfo.frames[l_FrameI];
                l_Delays[l_FrameI] = l_CurrentFrameInfo.delay;

                var l_FrameTexture = new Texture2D(l_CurrentFrameInfo.width, l_CurrentFrameInfo.height, TextureFormat.RGBA32, false);
                l_FrameTexture.wrapMode = TextureWrapMode.Clamp;

                try
                {
                    l_FrameTexture.SetPixels32(l_CurrentFrameInfo.colors);
                    l_FrameTexture.Apply(l_FrameI == 0);
                }
                catch
                {
                    yield break;
                }

                yield return null;

                l_SubTextures[l_FrameI] = l_FrameTexture;

                if (l_FrameI == 0)
                {
                    width = p_AnimationInfo.frames[l_FrameI].width;
                    height = p_AnimationInfo.frames[l_FrameI].height;
                }
            }

            Rect[] atlas = l_AtlasTexture.PackTextures(l_SubTextures, 2, l_MaxAtlasTextureSize, true);

            foreach(Texture2D frameTex in l_SubTextures)
                GameObject.Destroy(frameTex);

            yield return null;

            callback?.Invoke(l_AtlasTexture, atlas, l_Delays, width, height);
        }

        private static int GetMaxAtlasTextureSize(AnimationInfo frameInfo, int i)
        {
            int testNum = 2;
            int numFramesInRow;
            int numFramesInColumn;

            while (true)
            {
                int numFrames = frameInfo.frameCount;

                // Make sure the number of frames is cleanly divisible by our testNum
                if (!(numFrames % testNum != 0))
                    numFrames += numFrames % testNum;

                numFramesInRow = Mathf.Max(1, numFrames / testNum);
                numFramesInColumn = numFrames / numFramesInRow;

                if (numFramesInRow <= numFramesInColumn)
                    break;

                testNum += 2;
            }

            int textureWidth = Mathf.Clamp(numFramesInRow * frameInfo.frames[i].width, 0, 2048);
            int textureHeight = Mathf.Clamp(numFramesInColumn * frameInfo.frames[i].height, 0, 2048);

            return Mathf.Max(textureWidth, textureHeight);
        }
    }
}