using UnityEngine;

public class Records
{
    public string player;
    public float mission1Points = 0f;
    public float mission2Points = 0f;
    public float mission3Points = 0f;
    public float total;

    public Records()
    {

    }

    public Records(string player, float mission1Points, float mission2Points, float mission3Points)
    {
        this.player = player;
        this.mission1Points = mission1Points;
        this.mission2Points = mission2Points;
        this.mission3Points = mission3Points;
        this.total = mission1Points + mission2Points + mission3Points;
        ProfileManager.Instance.AddScoreToCurrentProfile((int)total);
    }
}
