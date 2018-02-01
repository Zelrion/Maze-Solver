using UnityEngine;
using UnityEngine.UI;
public class MazeStatistics:MonoBehaviour {
	private void OnEnable() { MazeSearch.UpdateStatistics += UpdateStatistics; } //OnEnable, Subscribe to Event
	private void OnDisable() { MazeSearch.UpdateStatistics -= UpdateStatistics; } //OnDisable, Unsubscribe from Event
	private void UpdateStatistics(string stats) { GetComponent<Text>().text = stats; }
}
