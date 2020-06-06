package com.easy.bugly;

import android.os.Bundle;
import android.view.Window;

import com.tencent.bugly.crashreport.CrashReport;
import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;

public class GamePlayerActivity extends UnityPlayerActivity
{
    @Override protected void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);

        CrashReport.initCrashReport(getApplicationContext(),"android app id", false);
    }
}
