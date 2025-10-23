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

package assets.oculus.avatar.example;

import android.content.Context;
import android.content.pm.PackageManager;
import com.unity3d.player.UnityPlayer;

public class AvatarEditorOSHelper {

  /** Returns a version like '83.0.0.0.85' */
  public static String GetOSVersion() {
    Context context = UnityPlayer.currentActivity;
    PackageManager pm = context.getPackageManager();
    try {
      return pm.getPackageInfo("com.oculus.systemdriver", PackageManager.GET_META_DATA).versionName;
    } catch (Exception e) {
      // Return a default version if we can't get the version.
      return "0.0.0.0.0";
    }
  }
}
