using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

#if   UNITY_IOS
        BuglyAgent.InitWithAppId ("ios app id");
#elif UNITY_ANDROID
		//GamePlayerActivity已初始化，此处不需要再调用。
#endif

        BuglyAgent.ConfigDebugMode(false);
        BuglyAgent.EnableExceptionHandler();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
