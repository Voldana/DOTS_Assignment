using TMPro;
using Unity.Entities;
using UnityEngine;

namespace Project.Scripts.Mono
{
    public class ScoreManager: MonoBehaviour
    {
        [SerializeField] private TMP_Text score;
        public static ScoreManager Instance { get; private set; }
        private int Score { get; set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void AddScore(int points)
        {
            if (points <= 0) return;
            Score += points;
            score.text = Score.ToString();
        }
    }
    
    public struct ScoreEvent : IBufferElementData
    {
        public int points;
    }

}