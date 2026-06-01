using UnityEngine;

namespace ScriptableObjects {
    [CreateAssetMenu()]
    public class EnemySO : ScriptableObject {
        public string enemyName;
        public int enemyHealth = 60;
        public int enemyDamageAmount = 10;
        public int scoreValue = 10;
    }
}
