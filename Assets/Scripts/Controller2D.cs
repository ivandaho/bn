using UnityEngine;
using System.Collections;

public class Controller2D : RaycastController {
	
    // buttonpressing stuff
    SequenceInfo sequence;
    bool printed = false;
    bool generated = false;
    bool donepressing = false;
        
	float maxClimbAngle = 80;
	float maxDescendAngle = 80;
	
	public CollisionInfo collisions;
    [HideInInspector]
    public Vector2 playerInput;


	public override void Start() {
		base.Start ();
		collisions.faceDir = 1;

	}
    
	public void Move(Vector3 velocity, bool standingOnPlatform = false) {
        Move (velocity, Vector2.zero, standingOnPlatform);
    }

	public void Move(Vector3 velocity, Vector2 input, bool standingOnPlatform = false) {
		UpdateRaycastOrigins ();
		collisions.Reset ();
		collisions.velocityOld = velocity;
        playerInput = input;



		if (velocity.x != 0) {
			collisions.faceDir = (int)Mathf.Sign(velocity.x);
		}

		if (velocity.y < 0) {
			DescendSlope(ref velocity);
		}

		HorizontalCollisions (ref velocity);

		if (velocity.y != 0) {
			VerticalCollisions (ref velocity);
		}

		transform.Translate (velocity);

		if (standingOnPlatform) {
			collisions.below = true;
		}
        if (donepressing) {
            velocity = collisions.velocityOld;
        }
           
	}

	void HorizontalCollisions(ref Vector3 velocity) {
		float directionX = collisions.faceDir;
		float rayLength = Mathf.Abs (velocity.x) + skinWidth;

		if (Mathf.Abs(velocity.x) < skinWidth) {
			rayLength = 2*skinWidth;
		}
		
		for (int i = 0; i < horizontalRayCount; i ++) {
			Vector2 rayOrigin = (directionX == -1)?raycastOrigins.bottomLeft:raycastOrigins.bottomRight;
			rayOrigin += Vector2.up * (horizontalRaySpacing * i);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

			Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength,Color.red);

			if (hit) {

                if (hit.collider.tag == "Checker") {
                    generateSequence();
                    generated = true;
                    continue;
                }

				if (hit.distance == 0) {
					continue;
				}

			
				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

				if (i == 0 && slopeAngle <= maxClimbAngle) {
					if (collisions.descendingSlope) {
						collisions.descendingSlope = false;
						velocity = collisions.velocityOld;
					}
					float distanceToSlopeStart = 0;
					if (slopeAngle != collisions.slopeAngleOld) {
						distanceToSlopeStart = hit.distance-skinWidth;
						velocity.x -= distanceToSlopeStart * directionX;
					}
					ClimbSlope(ref velocity, slopeAngle);
					velocity.x += distanceToSlopeStart * directionX;
				}

				if (!collisions.climbingSlope || slopeAngle > maxClimbAngle) {
					velocity.x = (hit.distance - skinWidth) * directionX;
					rayLength = hit.distance;

					if (collisions.climbingSlope) {
						velocity.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
					}

					collisions.left = directionX == -1;
					collisions.right = directionX == 1;
				}
			}
		}
	}
	
	void VerticalCollisions(ref Vector3 velocity) {
		float directionY = Mathf.Sign (velocity.y);
		float rayLength = Mathf.Abs (velocity.y) + skinWidth;

		for (int i = 0; i < verticalRayCount; i ++) {

			Vector2 rayOrigin = (directionY == -1)?raycastOrigins.bottomLeft:raycastOrigins.topLeft;
			rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

			Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength,Color.red);

			if (hit) {
                if (hit.collider.tag == "Hurt") {
                        print("hurt!");
                }

                if (hit.collider.tag == "Checker") {
                    generateSequence();
                    generated = true;
                    continue;
                }
                if (hit.collider.tag == "Through") {
                    if (directionY == 1 || hit.distance == 0) {
                        continue;
                    }
                    if (collisions.fallingThroughPlatform) {
                        continue;
                    }
                    if (playerInput.y == -1) {
                        collisions.fallingThroughPlatform = true;
                        Invoke("ResetFallingThroughPlatform", .5f);
                        continue;
                    }
                }
			 
				velocity.y = (hit.distance - skinWidth) * directionY;
				rayLength = hit.distance;

				if (collisions.climbingSlope) {
					velocity.x = velocity.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
				}

				collisions.below = directionY == -1;
				collisions.above = directionY == 1;
			}
		}

		if (collisions.climbingSlope) {
			float directionX = Mathf.Sign(velocity.x);
			rayLength = Mathf.Abs(velocity.x) + skinWidth;
			Vector2 rayOrigin = ((directionX == -1)?raycastOrigins.bottomLeft:raycastOrigins.bottomRight) + Vector2.up * velocity.y;
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin,Vector2.right * directionX,rayLength,collisionMask);

			if (hit) {
				float slopeAngle = Vector2.Angle(hit.normal,Vector2.up);
				if (slopeAngle != collisions.slopeAngle) {
					velocity.x = (hit.distance - skinWidth) * directionX;
					collisions.slopeAngle = slopeAngle;
				}
			}
		}
	}

	void ClimbSlope(ref Vector3 velocity, float slopeAngle) {
		float moveDistance = Mathf.Abs (velocity.x);
		float climbVelocityY = Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance;

		if (velocity.y <= climbVelocityY) {
			velocity.y = climbVelocityY;
			velocity.x = Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign (velocity.x);
			collisions.below = true;
			collisions.climbingSlope = true;
			collisions.slopeAngle = slopeAngle;
		}
	}

	void DescendSlope(ref Vector3 velocity) {
		float directionX = Mathf.Sign (velocity.x);
		Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
		RaycastHit2D hit = Physics2D.Raycast (rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

		if (hit) {
			float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
			if (slopeAngle != 0 && slopeAngle <= maxDescendAngle) {
				if (Mathf.Sign(hit.normal.x) == directionX) {
					if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x)) {
						float moveDistance = Mathf.Abs(velocity.x);
						float descendVelocityY = Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance;
						velocity.x = Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign (velocity.x);
						velocity.y -= descendVelocityY;

						collisions.slopeAngle = slopeAngle;
						collisions.descendingSlope = true;
						collisions.below = true;
					}
				}
			}
		}
	}

    void ResetFallingThroughPlatform() {
        collisions.fallingThroughPlatform = false;
    }

    void Update() {
        if(generated) {
            if (sequence.pressProgress == 0) {
                CheckInput(sequence.buttonChain.Substring(0,1));
            } else if (sequence.pressProgress == 1) {
                CheckInput(sequence.buttonChain.Substring(1,1));
            } else if (sequence.pressProgress == 2) {
                CheckInput(sequence.buttonChain.Substring(2,1));
            } else if (sequence.pressProgress == 3) {
                CheckInput(sequence.buttonChain.Substring(3,1));
            }

            if (sequence.pressProgress == sequence.pressLength &&
                    printed == false) {
                print("sequence completed!!!");
                printed = true;
                donepressing = true;
                //unPause(ref velocity);
            }
        }
    }        
    void unPause(ref Vector3 velocity) {
            velocity = collisions.velocityOld;
    }

    void generateSequence() {
        if(!generated) {
            sequence.pressLength = 4;
            sequence.pressProgress = 0;
            RandButtonChain(sequence.pressLength);
            generated = true;
        }
    }
    void RandButtonChain(int n) {

        // 1 = up, 2 = down, 3 = left, 4 = right
        System.Random rnd = new System.Random();

        for (int i=0; i<n; i++) {

            int gen = rnd.Next(1,5); // does not include MaxValue

            sequence.buttonChain = sequence.buttonChain + gen.ToString();
        }
        BuildRequiredPressSequence(n);
    }

    void BuildRequiredPressSequence(int n) {
        for (int i=0; i<n; i++) {
            if (sequence.buttonChain.Substring(i,1) == "1"){
                print("UP");
            } else if (sequence.buttonChain.Substring(i,1) == "2") {
                print("DOWN");
            } else if (sequence.buttonChain.Substring(i,1) == "3") {
                print("LEFT");
            } else if (sequence.buttonChain.Substring(i,1) == "4") {
                print("RIGHT");
            } 
        }
    }

    void CheckInput(string s) {
        if (Input.GetKeyDown(ConvertIndexToKey(s))) {
            print(s + " key pressed");
            sequence.pressProgress += 1;
        }
    }

    string ConvertIndexToKey(string s) {
        if (s == "1") {
            return "w";
        }
        if (s == "2") {
            return "s";
        }
        if (s == "3") {
            return "a";
        }
        if (s == "4") {
            return "d";
        }
        return "0";
    }


    void CheckRequiredPressSequence() {
        if (Input.GetKeyDown(KeyCode.W)) {
            print("w pressed, complete = true");
            sequence.complete = true;
        }
    }

    struct SequenceInfo {
        public bool complete;
        public string buttonChain;
        public int pressLength;
        public int pressProgress;
    }
    


	public struct CollisionInfo {
		public bool above, below;
		public bool left, right;

		public bool climbingSlope;
		public bool descendingSlope;
		public float slopeAngle, slopeAngleOld;
		public Vector3 velocityOld;
		public int faceDir;
        public bool fallingThroughPlatform;

		public void Reset() {
			above = below = false;
			left = right = false;
			climbingSlope = false;
			descendingSlope = false;

			slopeAngleOld = slopeAngle;
			slopeAngle = 0;
		}
	}

}
