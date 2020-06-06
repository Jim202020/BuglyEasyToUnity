using UnityEngine;
public class Main : MonoBehaviour
{
    void Start()
    {

#if   UNITY_IOS
        BuglyAgent.InitWithAppId ("your ios app id");
#elif UNITY_ANDROID
		//GamePlayerActivity已初始化，此处不需要再调用。
#endif

        BuglyAgent.ConfigDebugMode(false);
        BuglyAgent.EnableExceptionHandler();

        Test();
    }

    void Test()
    {
        print("Test Bugly NullReferenceException");
        GameObject go = null;
        go.name = "";
    }
}
