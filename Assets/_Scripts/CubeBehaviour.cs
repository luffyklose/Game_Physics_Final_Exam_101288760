using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Color = UnityEngine.Color;

[System.Serializable]
public class Contact : IEquatable<Contact>
{
   

    public CubeBehaviour cube;
    public Vector3 face;
    public float penetration;

    public Contact(CubeBehaviour cube)
    {
        this.cube = cube;
        face = Vector3.zero;
        penetration = 0.0f;
    }

    public override bool Equals(object obj)
    {
        if (obj == null) return false;
        Contact objAsContact = obj as Contact;
        if (objAsContact == null) return false;
        else return Equals(objAsContact);
    }

    public override int GetHashCode()
    {
        return this.cube.gameObject.GetInstanceID();
    }

    public bool Equals(Contact other)
    {
        if (other == null) return false;

        return (
            (this.cube.gameObject.name.Equals(other.cube.gameObject.name)) &&
            (this.face == other.face) &&
            (Mathf.Approximately(this.penetration, other.penetration))
            );
    }

    public override string ToString()
    {
        return "Cube Name: " + cube.gameObject.name + " face: " + face.ToString() + " penetration: " + penetration;
    }
}


[System.Serializable]
public class CubeBehaviour : MonoBehaviour
{
    public enum CubeType
    {
        Unknown,
        Block,
        Player,
        Bullet,
        Ground
    };

    [Header("Cube Attributes")]
    public Vector3 size;
    public Vector3 max;
    public Vector3 min;
    public bool isColliding;
    public bool debug;
    public List<Contact> contacts;
    public float PushSpeed;
    public CubeType type = CubeType.Unknown;

    public Vector3 collisionNormal;
    public Vector3 direction;
    public float speed;
    public float range;

    public BulletManager bulletManager;

    private MeshFilter meshFilter;
    public Bounds bounds;
    public bool isGrounded;


    // Start is called before the first frame update
    void Start()
    {
        debug = false;
        meshFilter = GetComponent<MeshFilter>();

        bounds = meshFilter.mesh.bounds;
        size = bounds.size;

        PushSpeed = 0.5f;

        if (type == CubeType.Bullet)
        {
            isColliding = false;
            bulletManager = FindObjectOfType<BulletManager>();
        }
            
    }

    // Update is called once per frame
    void Update()
    {
        if (type == CubeType.Bullet)
        {
            _Move();
            _CheckBounds();
        }

        for (int i = 0; i < contacts.Count; i++)
        {
            if (type == CubeType.Bullet || contacts[i].cube.type==CubeType.Bullet)
                break;
            if (GetComponent<RigidBody3D>().bodyType == BodyType.DYNAMIC)
            {
                if (contacts[i].cube.gameObject.GetComponent<RigidBody3D>().bodyType == BodyType.DYNAMIC)
                {
                    Vector3 MoveDir = contacts[i].penetration * contacts[i].face;
                    transform.position -= 0.015f * MoveDir;
                    contacts[i].cube.transform.position += 0.1f * MoveDir;
                }
                else
                {
                    transform.position -= contacts[i].penetration * contacts[i].face;
                }
            }
        }       

        max = Vector3.Scale(bounds.max, transform.localScale) + transform.position;
        min = Vector3.Scale(bounds.min, transform.localScale) + transform.position;
    }

    private void OnDrawGizmos()
    {
        if (debug)
        {
            Gizmos.color = Color.magenta;

            Gizmos.DrawWireCube(transform.position, Vector3.Scale(new Vector3(1.0f, 1.0f, 1.0f), transform.localScale));
        }
    }

    public void PushCube(CubeBehaviour block)
    {
        Debug.Log("Old Loc: " + block.transform.position);
        Vector3 MoveDir = block.transform.position - transform.position;
        MoveDir.y = 0;

        block.transform.position += MoveDir * PushSpeed;

        Debug.Log("move dir" + MoveDir + " Push Speed" + PushSpeed + " final vel: " + MoveDir * PushSpeed);
        Debug.Log("New Loc " + block.transform.position);
    }

    private void _Move()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void _CheckBounds()
    {
        if (Vector3.Distance(transform.position, Vector3.zero) > range)
        {
            bulletManager.ReturnBullet(this.gameObject);
        }
    }
}
