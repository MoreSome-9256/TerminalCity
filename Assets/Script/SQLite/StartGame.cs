using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGame : MonoBehaviour
{
    void Start()
    {
        TestDb();
    }
    private void TestDb()
    {
        string dbPath = Application.streamingAssetsPath + "/dialog_global.db";
        SqlDbConnect sqlDbConnect = new SqlDbConnect(dbPath);
    }
}
