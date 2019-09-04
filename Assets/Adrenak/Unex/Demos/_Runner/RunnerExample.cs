using UnityEngine;
using Adrenak.Unex;
using System.Collections;

public class RunnerExample : MonoBehaviour {
	IEnumerator Start() {
		var runner = Runner.New(CoroutineA1());
		runner.OnStateChange += state => Debug.Log(state);
		runner.Run();

		yield return new WaitForSeconds(.5f);
		runner.Pause();
		yield return new WaitForSeconds(.5f);
		runner.Resume();
		yield return new WaitForSeconds(2f);
		runner.Destroy();
		Runner.New().WaitForSeconds(2, () => Debug.Log("Printed after 2 seconds"));

		Runner.New().WaitUntil(() => Input.GetKeyDown(KeyCode.S))
			.Then(s => {
				Debug.Log("Pressed S");
				return s.WaitUntil(() => Input.GetKeyDown(KeyCode.D));
			})
			.Then(d => {
				Debug.Log("Pressed D");
				return d.WaitUntil(() => Input.GetKeyDown(KeyCode.F));
			})
			.Then(f => {
				Debug.Log("Pressed F");
				Debug.Log("Combo over");
			})
			.Then(() => {
				Runner.New().RunIf(
					() => Input.mousePosition.y > Screen.height / 2,
					() => Debug.Log("Mouse in the top half")
				);
			});

    }

	IEnumerator CoroutineA1() {
		Debug.Log("Starting coroutine");
        yield return new WaitForSeconds(2);
		Debug.Log("Done coroutine");

	}
}
