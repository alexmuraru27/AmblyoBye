using UnityEngine;

public class MoveMovieObject : MonoBehaviour
{
    public GameObject movieScreen;
    public GameObject grabobject;

    public Vector3 positionOffset;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        movieScreen.transform.position = grabobject.transform.position + positionOffset;
    }
}
