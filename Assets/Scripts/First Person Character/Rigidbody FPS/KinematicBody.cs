using UnityEngine;
using System.Collections.Generic;

// Code from https://github.com/marmitoTH/Unity-Kinematic-Body
[RequireComponent(typeof(CapsuleCollider), typeof(Rigidbody))]
public class KinematicBody : MonoBehaviour
{
	public float slopeLimit = 45f;
	public float stepOffset = 0.3f;
	public float skinWidth = 0.08f;

	[SerializeField] private Vector3 m_center = Vector3.zero;
	[SerializeField] private float m_radius = 0.5f;
	[SerializeField] private float m_height = 2f;

	private Vector3 m_position;
	private Vector3 m_upDirection;

	private Rigidbody m_rigidbody;
	private CapsuleCollider m_collider;

	private readonly Collider[] m_overlaps = new Collider[5];
	private readonly List<RaycastHit> m_contacts = new List<RaycastHit>();

	private const int MaxSweepSteps = 5;
	private const float MinMoveDistance = 0f;
	private const float MinCeilingAngle = 145;

	public Vector3 velocity { get; set; }
	public bool isGrounded { get; private set; }

	public Vector3 center
	{
		get { return m_center; }
		set
		{
			m_center = value;
			m_collider.center = value;
		}
	}

	public float radius
	{
		get { return m_radius; }
		set
		{
			m_radius = value;
			m_collider.radius = value;
		}
	}

	public float height
	{
		get { return m_height; }
		set
		{
			m_height = value;
			m_collider.height = value;
		}
	}

	private void Start()
	{
		InitializeRigidbody();
		InitializeCollider();
	}

	private void InitializeRigidbody()
	{
		m_rigidbody = GetComponent<Rigidbody>();
		m_rigidbody.isKinematic = true;
		m_rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
	}

	private void InitializeCollider()
	{
		m_collider = GetComponent<CapsuleCollider>();
		m_collider.center = m_center;
		m_collider.height = m_height;
		m_collider.radius = m_radius;
	}

	public void Move(Vector3 motion)
	{
		GetState(motion);
		ClearVariables();
		HandleCollision();
		HandleContacts();
		Depenetrate();
		SetState();
	}

	public void Rotate(Quaternion rotation)
	{
		m_rigidbody.MoveRotation(rotation);
	}

	private void GetState(Vector3 motion)
	{
		m_position = m_rigidbody.position;
		m_upDirection = transform.up;
		velocity = motion;
	}

	private void SetState()
	{
		m_rigidbody.MovePosition(m_position);
	}

	private void ClearVariables()
	{
		m_contacts.Clear();
		isGrounded = false;
	}

	private void HandleCollision()
	{
		if (velocity.sqrMagnitude > MinMoveDistance)
		{
			Vector3 localVelocity = transform.InverseTransformDirection(velocity) * Time.deltaTime;
			Vector3 lateralVelocity = new Vector3(localVelocity.x, 0, localVelocity.z);
			Vector3 verticalVelocity = new Vector3(0, localVelocity.y, 0);

			lateralVelocity = transform.TransformDirection(lateralVelocity);
			verticalVelocity = transform.TransformDirection(verticalVelocity);

			CapsuleSweep(lateralVelocity.normalized, lateralVelocity.magnitude, stepOffset, MinCeilingAngle);
			CapsuleSweep(verticalVelocity.normalized, verticalVelocity.magnitude, 0, 0, slopeLimit);
		}
	}

	private void HandleContacts()
	{
		if (m_contacts.Count > 0)
		{
			float angle;

			foreach (RaycastHit contact in m_contacts)
			{

				angle = Vector3.Angle(m_upDirection, contact.normal);

				if (angle <= slopeLimit)
				{
					isGrounded = true;
				}

				velocity -= Vector3.Project(velocity, contact.normal);
			}
		}
	}

	private void CapsuleSweep(Vector3 direction, float distance, float stepOffset, float minSlideAngle = 0, float maxSlideAngle = 360)
	{
		Vector3 origin, top, bottom;
		RaycastHit hitInfo;
		float safeDistance;
		float slideAngle;

		float capsuleOffset = m_height * 0.5f - m_radius;

		for (int i = 0; i < MaxSweepSteps; i++)
		{
			origin = m_position + m_center - direction * m_radius;
			bottom = origin - m_upDirection * (capsuleOffset - stepOffset);
			top = origin + m_upDirection * capsuleOffset;

			if (Physics.CapsuleCast(top, bottom, m_radius, direction, out hitInfo, distance + m_radius))
			{
				slideAngle = Vector3.Angle(m_upDirection, hitInfo.normal);
				safeDistance = hitInfo.distance - m_radius - skinWidth;
				m_position += direction * safeDistance;
				m_contacts.Add(hitInfo);

				if ((slideAngle >= minSlideAngle) && (slideAngle <= maxSlideAngle))
				{
					break;
				}

				direction = Vector3.ProjectOnPlane(direction, hitInfo.normal);
				distance -= safeDistance;
			}
			else
			{
				m_position += direction * distance;
				break;
			}
		}
	}

	private void Depenetrate()
	{
		float capsuleOffset = m_height * 0.5f - m_radius;
		Vector3 top = m_position + m_upDirection * capsuleOffset;
		Vector3 bottom = m_position - m_upDirection * capsuleOffset;
		int overlapsNum = Physics.OverlapCapsuleNonAlloc(top, bottom, m_collider.radius, m_overlaps);

		if (overlapsNum > 0)
		{
			for (int i = 0; i < overlapsNum; i++)
			{
				if ((m_overlaps[i].transform != transform) && Physics.ComputePenetration(m_collider, m_position, transform.rotation, 
					m_overlaps[i], m_overlaps[i].transform.position, m_overlaps[i].transform.rotation, out Vector3 direction, out float distance))
				{
					m_position += direction * (distance + skinWidth);
					velocity -= Vector3.Project(velocity, -direction);
				}
			}
		}
	}
}
