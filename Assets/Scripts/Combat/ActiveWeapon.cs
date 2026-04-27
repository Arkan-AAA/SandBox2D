using UnityEngine;

public class ActiveWeapon : MonoBehaviour
{
    public static ActiveWeapon Instance { get; private set; }
    
    private void Awake()
    {
        Instance = this;
    }
    
    [SerializeField] private Sword sword;

    public Sword GetActiveWeapon()
    {
        return sword;
    }
}
