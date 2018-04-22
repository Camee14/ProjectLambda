using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxLayer : MonoBehaviour {
    public SpriteRenderer[] Images;
    public float Speed;
    int left_index, center_index, right_index;
    Vector3 p_camera_center;
    void Start() {
        left_index = 0;
        center_index = 1;
        right_index = 2;

        transform.position = new Vector3(Camera.main.transform.position.x, transform.position.y, transform.position.z);
        p_camera_center = transform.position;
        p_camera_center.y = transform.position.y;
        p_camera_center.z = transform.position.z;

        Images[left_index].transform.position = new Vector2(
            transform.position.x - Images[center_index].sprite.bounds.size.x,
            transform.position.y
        );
        Images[center_index].transform.position = new Vector2(
            transform.position.x,
            transform.position.y
        );
        Images[right_index].transform.position = new Vector2(
            transform.position.x + Images[center_index].sprite.bounds.size.x,
            transform.position.y
        );
    }
    void Update() {
        Vector3 camera_center = Camera.main.transform.position;
        camera_center.y = transform.position.y;
        camera_center.z = transform.position.z;

        if (p_camera_center != camera_center) {
            Vector3 dir = (camera_center - p_camera_center);
            dir.y = 0;
            foreach (SpriteRenderer sprite in Images) {
                sprite.transform.position += -dir * Speed * Time.deltaTime;
            }
        }
        
        p_camera_center = camera_center;

        if (Images[left_index].sprite.bounds.Contains(Images[left_index].transform.InverseTransformPoint(camera_center))) {
             Images[right_index].transform.position = new Vector2(
                 Images[left_index].transform.position.x - Images[right_index].sprite.bounds.size.x,
                 Images[left_index].transform.position.y);

             int temp = left_index;
             left_index = right_index;
             right_index = center_index;
             center_index = temp;

        }
        else if (Images[right_index].sprite.bounds.Contains(Images[right_index].transform.InverseTransformPoint(camera_center))) {
            Images[left_index].transform.position = new Vector2(
                Images[right_index].transform.position.x + Images[left_index].sprite.bounds.size.x,
                Images[right_index].transform.position.y);

            int temp = right_index;
            right_index = left_index;
            left_index = center_index;
            center_index = temp;
        }
    }
}
