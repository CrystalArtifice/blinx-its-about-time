using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // The player to follow.
    public GameObject player;

    // The offset between the camera and the player's position.
    private Vector3 offset;

    void Start ()
    {
        offset = transform.position - player.transform.position;
    }

	// Update is called once per frame
	void LateUpdate ()
    {
        transform.position = player.transform.position + offset;
	}
}
