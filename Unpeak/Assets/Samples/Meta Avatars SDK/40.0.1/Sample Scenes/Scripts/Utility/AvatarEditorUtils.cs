/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * Licensed under the Oculus SDK License Agreement (the "License");
 * you may not use the Oculus SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 *
 * You may obtain a copy of the License at
 *
 * https://developer.oculus.com/licenses/oculussdk/
 *
 * Unless required by applicable law or agreed to in writing, the Oculus SDK
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#nullable enable

using System;
#if UNITY_ANDROID && !UNITY_EDITOR
using System.Text.RegularExpressions;
#endif // UNITY_ANDROID && !UNITY_EDITOR
using Oculus.Platform;
using UnityEngine;

namespace Oculus.Avatar2
{
    // This class is used for backwards compatibility
    public class AvatarEditorDeeplink
    {
        public static void LaunchAvatarEditor()
        {
            AvatarEditorUtils.LaunchAvatarEditor();
        }
    }
    public class AvatarEditorUtils
    {

#if UNITY_ANDROID && !UNITY_EDITOR
        private static string IS_OPENED_BY_SDK2_APP_KEY = "isOpenedBySdk2App";
        private static string IS_STYLE_2_ELIGIBLE_KEY = "isStyle2Eligible";
        private static string AVATAR_STYLE_ID_KEY = "avatarStyleID";
        private static string VERSION_KEY = "version";
#endif // UNITY_ANDROID && !UNITY_EDITOR
        public static void LaunchAvatarEditor()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
            AvatarEditorOptions options = new AvatarEditorOptions();
            options.SetSourceOverride("avatar_2_sdk");
            var result = new Request<Oculus.Platform.Models.AvatarEditorResult>(Oculus.Platform.CAPI.ovr_Avatar_LaunchAvatarEditor((IntPtr)options));
#elif UNITY_ANDROID
            try
            {
                int osMajorVersion = GetMajorOsVersion();
                if (osMajorVersion >= 82)
                {
                    try
                    {
                        string action = "com.oculus.avatareditor.action.ACTION_AVATAR_EDITOR";
                        using AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", action);
                        intent.Call<AndroidJavaObject>("setPackage", "com.oculus.avatareditor");
                        string uri = "/?route=/style2MaxSwitching";
                        intent.Call<AndroidJavaObject>("putExtra", "uri", uri);

                        int FLAG_ACTIVITY_NEW_TASK = new AndroidJavaClass("android.content.Intent").GetStatic<int>("FLAG_ACTIVITY_NEW_TASK");
                        intent.Call<AndroidJavaObject>("setFlags", FLAG_ACTIVITY_NEW_TASK);
                        using AndroidJavaObject unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                        using AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                        currentActivity.Call("startActivity", intent);
                    }
                    catch (AndroidJavaException exception)
                    {
                        LaunchAvatarEditorViaActivity();
                    }
                } else {
                    LaunchAvatarEditorViaActivity();
                }
            }
            catch (Exception error)
            {
                OvrAvatarLog.LogError("[AvatarEditorUtils] Launch Avatar Editor error: " + error);
            }
#endif // UNITY_ANDROID
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private static void LaunchAvatarEditorViaActivity()
        {
            AndroidJavaObject activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = activityClass.GetStatic<AndroidJavaObject>("currentActivity");

            var intent = new AndroidJavaObject("android.content.Intent");
            intent.Call<AndroidJavaObject>("setClassName", "com.oculus.avatareditor", "com.oculus.avatareditor.AvatarEditorActivity");

            string packageName = currentActivity.Call<string>("getPackageName");
            var paramsBuilder = new IntentUriParamsBuilder();
            paramsBuilder.AddParam($"returnUrl=apk://{packageName}");
            paramsBuilder.AddParam(IS_OPENED_BY_SDK2_APP_KEY, "true");
            paramsBuilder.AddParam(IS_STYLE_2_ELIGIBLE_KEY, "true");
            paramsBuilder.AddParam(AVATAR_STYLE_ID_KEY, "STYLE_2");
            paramsBuilder.AddParam(VERSION_KEY, "V2");

            string uriExtra = paramsBuilder.ToString();
            intent.Call<AndroidJavaObject>(
                "putExtra",
                "uri",
                uriExtra
            );

            int FLAG_ACTIVITY_NEW_TASK = new AndroidJavaClass("android.content.Intent").GetStatic<int>("FLAG_ACTIVITY_NEW_TASK");

            intent.Call<AndroidJavaObject>("addFlags", FLAG_ACTIVITY_NEW_TASK);

            currentActivity.Call("startActivity", intent);
        }

        private static int GetMajorOsVersion()
        {
            int osMajorVersion = 1;
            try
            {
                using AndroidJavaClass osHelper = new AndroidJavaClass("assets.oculus.avatar.example.AvatarEditorOSHelper");
                string value = osHelper.CallStatic<string>("GetOSVersion");
                Match match = Regex.Match(value, @"\d+");
                string osMajorVersionString = match.Success ? match.Value : "1";
                if (int.TryParse(osMajorVersionString, out int parsedOsMajorVersion))
                {
                    osMajorVersion = parsedOsMajorVersion;
                }
                return osMajorVersion;
            }
            catch (Exception)
            {
                return osMajorVersion;
            }
        }
#endif // UNITY_ANDROID && !UNITY_EDITOR
    }
}
