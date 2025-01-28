using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace AdvancedRogueLikeandPuzzleSystem
{
    public class HeroController : MonoBehaviour
    {
        public SphereCollider sphereCollider;

        [Header("PROFILE DETAILS")]
        public string Name;
        public Sprite ProfileImage;
        public int Coin;
        public int Level;
        public int Mana = 100;
        public float TotalMana = 100;

        [Header("BODY DETAILS")]
        public int Health;
        public float TotalHealth;
        public float Speed;
        public float SprintSpeed;
        public int HitPower;
        public GameObject Sprite_Interaction;

        public int MaxHealthPot = 4;
        public int CurrentHealthPot = 0;
        public int MaxManaPot = 4;
        public int CurrentManaPot = 0;

        public GameObject InteractedItem;
        public ItemType InteractedItemType;
        public Animator Animator;

        public Transform playerCameraRoot;
        public CharacterController characterController;
        public ThirdPersonController thirdPersonController;
        private Collider cold;

        public static HeroController instance;

        public GameObject WeaponAttack;
        public GameObject WeaponDefend;

        private bool WeaponsonHands = false;
        private bool ShieldonHands = false;
        public BodyPart bodyPart;
        public GameObject bodyVisual;
        [HideInInspector]
        public bool inDefendMode = false;
        private int Hitindex = 0;
        public float HittingPeriod = 1.5f;
        [HideInInspector]
        public List<int> Keys_Grabbed = new List<int>();
        [HideInInspector]
        public List<PotionType> Potions_Grabbed = new List<PotionType>();
        [HideInInspector]
        public List<int> Potions_Parameters = new List<int>();
        public GameObject BodyShield;
        [HideInInspector]
        public bool isInvisible = false;
        public AudioSource audio_AyakSesi;

        private void Awake()
        {
            instance = this;
            TotalHealth = Health;
            InitPlayer();
        }

        public void InitPlayer()
        {
            Animator = GetComponent<Animator>();
            Level = PlayerPrefs.GetInt("Level", 1);
            Coin = PlayerPrefs.GetInt("Coin", 0);
            cold = GetComponent<Collider>();
        }


        void Start()
        {
            TotalMana = Mana;
            RefreshProperties();
            characterController = GetComponent<CharacterController>();
            thirdPersonController = GetComponent<ThirdPersonController>();
            thirdPersonController.MoveSpeed = Speed;
            thirdPersonController.SprintSpeed = SprintSpeed;
            GameCanvas_Controller.instance.Update_ProfileDetails(Name, ProfileImage);
        }

        public void Grab_Key(int ID)
        {
            Keys_Grabbed.Add(ID);
        }

        public bool AddPotion(PotionType potionType, int parameter)
        {
            if (potionType == PotionType.Health)
            {
                if (MaxHealthPot > CurrentHealthPot)
                {
                    CurrentHealthPot++;
                    GameCanvas_Controller.instance.Image_Health_Potion.GetComponent<Image>().fillAmount = CurrentHealthPot * 0.25f;
                    Potions_Grabbed.Add(potionType);
                    Potions_Parameters.Add(parameter);
                    return true;
                }
            }
            else if (potionType == PotionType.Mana)
            {
                if (MaxManaPot > CurrentManaPot)
                {
                    CurrentManaPot++;
                    GameCanvas_Controller.instance.Image_Mana_Potion.GetComponent<Image>().fillAmount = CurrentManaPot * 0.25f;
                    Potions_Grabbed.Add(potionType);
                    Potions_Parameters.Add(parameter);
                    return true;
                }
            }
            return false;
        }

        public void RefreshProperties()
        {
            TotalHealth = Health;
            GameCanvas_Controller.instance.Update_Health_Bar();
        }

        public void ShowInteractSprite()
        {
            if (GameManager.Instance.controllerType == ControllerType.KeyboardMouse)
            {
                Sprite_Interaction.transform.LookAt(Camera.main.transform);
                Sprite_Interaction.SetActive(true);
            }
            else
            {
                GameCanvas_Controller.instance.Show_Button_Interact();
            }
        }



        public void HideInteractSprite()
        {
            if (GameManager.Instance.controllerType == ControllerType.KeyboardMouse)
            {
                Sprite_Interaction.SetActive(false);
            }
            else
            {
                GameCanvas_Controller.instance.Hide_Button_Interact();
            }
        }

        public void Hit()
        {
            if (!thirdPersonController.enabled) return;

            if ((WeaponsonHands && WeaponAttack != null) && inDefendMode == false && !isHitting)
            {
                Hitindex++;
                Animator.SetTrigger("Hit" + Hitindex.ToString());
                switch (Hitindex)
                {
                    case 1:
                        Animator.ResetTrigger("Hit2");
                        Animator.ResetTrigger("Hit3");
                        break;
                    case 2:
                        Animator.ResetTrigger("Hit1");
                        Animator.ResetTrigger("Hit3");
                        break;
                    case 3:
                        Animator.ResetTrigger("Hit1");
                        Animator.ResetTrigger("Hit2");
                        break;
                }
                isHitting = true;
                AudioManager.instance.Play_WeaponWhoosh();
                StopCoroutine(CutTheSpeed());
                StartCoroutine(CutTheSpeed());
                if (Hitindex >= 3)
                {
                    Hitindex = 0;
                }
            }
        }

        int power = 0;

        public void Hit1Effect()
        {
            HittingParticle_1.Play();
            WeaponEffectScript.Instance.Attack();
        }

        public void Hit2Effect()
        {
            HittingParticle_2.Play();
            WeaponEffectScript.Instance.Attack();
        }

        public void Hit3Effect()
        {
            HittingParticle_3.Play();
            WeaponEffectScript.Instance.Attack();
        }

        private float particle_hit_1_waittime = 0.3f;
        private float particle_hit_2_waittime = 0.25f;
        private float particle_hit_3_waittime = 0.55f;

        public ParticleSystem HittingParticle_1;
        public ParticleSystem HittingParticle_2;
        public ParticleSystem HittingParticle_3;

        public void Defend()
        {
            if (!thirdPersonController.enabled) return;
            if (ShieldonHands && WeaponDefend != null && Animator.GetBool("Defend") == false)
            {
                Animator.SetBool("Defend", true);
                thirdPersonController.MoveSpeed = 0;
                thirdPersonController.SprintSpeed = 0;
                inDefendMode = true;
            }
            else if (ShieldonHands && WeaponDefend != null && Animator.GetBool("Defend"))
            {
                Animator.SetBool("Defend", false);
                thirdPersonController.MoveSpeed = Speed;
                thirdPersonController.SprintSpeed = SprintSpeed;
                inDefendMode = false;
            }
        }

        public void CheckInteractionInputsForMobile()
        {
            if (InteractedItem != null)
            {
                if (InteractedItemType == ItemType.Key)
                {
                    HideInteractSprite();
                    InteractedItem.GetComponent<KeyScript>().GrabIt();
                }
                else if (InteractedItemType == ItemType.Potion)
                {
                    HideInteractSprite();
                    InteractedItem.GetComponent<PotionScript>().GrabIt();
                }
                else if (InteractedItemType == ItemType.Door)
                {
                    InteractedItem.GetComponent<DoorScript>().TryToOpen();
                }
                else if (InteractedItemType == ItemType.Chest)
                {
                    HideInteractSprite();
                    InteractedItem.GetComponent<ChestScript>().Open();
                }
                else if (InteractedItemType == ItemType.Elevator)
                {
                    HideInteractSprite();
                    InteractedItem.GetComponent<AsansorScript>().Activate();
                }
                else if (InteractedItemType == ItemType.Mirror)
                {
                    InteractedItem.GetComponent<AynaCarkScript>().Activate();
                }
                if (InteractedItemType == ItemType.Weapon)
                {
                    HideInteractSprite();
                    TaketheWeapon();
                }
            }
        }

        public void CheckInteractionInputs()
        {
            var interacted = (Input.GetKeyUp(GameManager.Instance.Keycode_Interact) || Input.GetButtonUp("Interact"));
            if (interacted && InteractedItem != null)
            {
                if (InteractedItemType == ItemType.Key)
                {
                    HideInteractSprite();
                    InteractedItem.GetComponent<KeyScript>().GrabIt();
                }
                else if (InteractedItemType == ItemType.Potion)
                {
                    HideInteractSprite();
                    InteractedItem.GetComponent<PotionScript>().GrabIt();
                }
                else if (InteractedItemType == ItemType.Door)
                {
                    if (InteractedItem != null && InteractedItem.GetComponent<DoorScript>() != null)
                    {
                        InteractedItem.GetComponent<DoorScript>().TryToOpen();
                    }
                }
                else if (InteractedItemType == ItemType.Chest)
                {
                    HideInteractSprite();
                    InteractedItem.GetComponent<ChestScript>().Open();
                }
                if (InteractedItemType == ItemType.Weapon)
                {
                    TaketheWeapon();
                }
            }
        }

        public void CheckWeaponInputs()
        {
            if ((Input.GetMouseButton(1) || Input.GetButton("Shield")) && ShieldonHands && WeaponDefend != null)
            {
                Animator.SetBool("Defend", true);
                thirdPersonController.MoveSpeed = 0;
                thirdPersonController.SprintSpeed = 0;
                inDefendMode = true;
            }
            else if ((Input.GetMouseButtonUp(1) || Input.GetButtonUp("Shield")) && ShieldonHands && WeaponDefend != null)
            {
                Animator.SetBool("Defend", false);
                thirdPersonController.MoveSpeed = Speed;
                thirdPersonController.SprintSpeed = SprintSpeed;
                inDefendMode = false;
            }
            if ((Input.GetMouseButtonUp(0) || Input.GetButtonUp("Fire")) && (WeaponsonHands && WeaponAttack != null) && inDefendMode == false && !isHitting)
            {
                power = HeroController.instance.HitPower;
                Hit();
            }

        }

        public void CheckPotionInputsForMobile(int i)
        {
            if (i == 1 && Health < TotalHealth)
            {
                if (Potions_Grabbed.Contains(PotionType.Health) && CurrentHealthPot > 0)
                {
                    int parameter = Potions_Parameters[Potions_Grabbed.IndexOf(PotionType.Health)];
                    AddHealth(parameter);
                    AudioManager.instance.Play_Potion_Dring();
                    Potions_Parameters.RemoveAt(Potions_Grabbed.IndexOf(PotionType.Health));
                    Potions_Grabbed.Remove(PotionType.Health);
                    CurrentHealthPot--;
                    GameCanvas_Controller.instance.Image_Health_Potion.GetComponent<Image>().fillAmount = CurrentHealthPot * 0.25f;
                }
            }
            if (i == 2 && Mana < TotalMana)
            {
                if (Potions_Grabbed.Contains(PotionType.Mana) && CurrentManaPot > 0)
                {
                    int parameter = Potions_Parameters[Potions_Grabbed.IndexOf(PotionType.Mana)];
                    AddMana(parameter);
                    AudioManager.instance.Play_Potion_Dring();
                    Potions_Parameters.RemoveAt(Potions_Grabbed.IndexOf(PotionType.Mana));
                    Potions_Grabbed.Remove(PotionType.Mana);
                    CurrentManaPot--;
                    GameCanvas_Controller.instance.Image_Mana_Potion.GetComponent<Image>().fillAmount = CurrentManaPot * 0.25f;
                }
            }
        }


        public void CheckPotionInputs()
        {
            if ((Input.GetKeyUp(GameManager.Instance.Keycode_Using_Potion_Heal) || Input.GetButtonUp("Potion")) && Health < TotalHealth)
            {
                if (Potions_Grabbed.Contains(PotionType.Health) && CurrentHealthPot > 0)
                {
                    int parameter = Potions_Parameters[Potions_Grabbed.IndexOf(PotionType.Health)];
                    AddHealth(parameter);
                    AudioManager.instance.Play_Potion_Dring();
                    Potions_Parameters.RemoveAt(Potions_Grabbed.IndexOf(PotionType.Health));
                    Potions_Grabbed.Remove(PotionType.Health);
                    CurrentHealthPot--;
                    GameCanvas_Controller.instance.Image_Health_Potion.GetComponent<Image>().fillAmount = CurrentHealthPot * 0.25f;
                }
            }
            if ((Input.GetKeyUp(GameManager.Instance.Keycode_Using_Potion_Mana) || Input.GetButtonUp("Potion")) && CurrentManaPot > 0)
            {
                if (Potions_Grabbed.Contains(PotionType.Mana))
                {
                    int parameter = Potions_Parameters[Potions_Grabbed.IndexOf(PotionType.Mana)];
                    AddMana(parameter);
                    AudioManager.instance.Play_Potion_Dring();
                    Potions_Parameters.RemoveAt(Potions_Grabbed.IndexOf(PotionType.Mana));
                    Potions_Grabbed.Remove(PotionType.Mana);
                    CurrentManaPot--;
                    GameCanvas_Controller.instance.Image_Mana_Potion.GetComponent<Image>().fillAmount = CurrentManaPot * 0.25f;
                }
            }
        }

        private void Update()
        {
            if (Health <= 0)
            {
                return;
            }

            if (gettingHit) return;

            if (GameManager.Instance.controllerType == ControllerType.KeyboardMouse)
            {
                CheckInteractionInputs();
                CheckWeaponInputs();
                CheckPotionInputs();
            }
        }
        private float LastManaUpdateTime = 0;

        public void ManaUpdate()
        {
            if (Time.time >= LastManaUpdateTime + 1.5f)
            {
                LastManaUpdateTime = Time.time;
                if (Mana < TotalMana)
                {
                    Mana += 5;
                    if (Mana > TotalMana)
                    {
                        Mana = 100;
                    }
                    GameCanvas_Controller.instance.Update_Mana_Bar(0);
                }
            }
        }

        [HideInInspector]
        public bool isHitting = false;
        private IEnumerator CutTheSpeed()
        {
            thirdPersonController.MoveSpeed = 0;
            thirdPersonController.SprintSpeed = 0;
            yield return new WaitForSeconds((Hitindex == 3 ? HittingPeriod * 2 : HittingPeriod));
            isHitting = false;
            thirdPersonController.MoveSpeed = Speed;
            thirdPersonController.SprintSpeed = SprintSpeed;
        }

        IEnumerator PutOnWeapons()
        {
            yield return new WaitForSeconds(0.35f);
            WeaponAttack.transform.parent = bodyPart.WeaponAttackPutOnParent;
            WeaponAttack.transform.localPosition = bodyPart.WeaponAttackPutOnTransform.localPosition;
            WeaponAttack.transform.localRotation = bodyPart.WeaponAttackPutOnTransform.localRotation;
        }


        IEnumerator PutOffWeapons()
        {
            yield return new WaitForSeconds(0.6f);
            WeaponAttack.transform.parent = bodyPart.WeaponAttackPutOffParent;
            WeaponAttack.transform.localPosition = bodyPart.WeaponAttackPutOffTransform.localPosition;
            WeaponAttack.transform.localRotation = bodyPart.WeaponAttackPutOffTransform.localRotation;
        }


        IEnumerator PutOnShield()
        {
            yield return new WaitForSeconds(0.35f);
            WeaponDefend.transform.parent = bodyPart.WeaponDefendPutOnParent;
            WeaponDefend.transform.localPosition = bodyPart.WeaponDefendPutOnTransform.localPosition;
            WeaponDefend.transform.localRotation = bodyPart.WeaponDefendPutOnTransform.localRotation;
        }

        IEnumerator PutOffShield()
        {
            yield return new WaitForSeconds(0.6f);
            inDefendMode = false;
            WeaponDefend.transform.parent = bodyPart.WeaponDefendPutOffParent;
            WeaponDefend.transform.localPosition = bodyPart.WeaponDefendPutOffTransform.localPosition;
            WeaponDefend.transform.localRotation = bodyPart.WeaponDefendPutOffTransform.localRotation;
        }



        public void GetActionOnWeapon()
        {
            if (thirdPersonController.isSwimming) return;

            if (WeaponAttack != null)
            {
                if (!WeaponsonHands)
                {
                    StartCoroutine(PutOnWeapons());
                    GameCanvas_Controller.instance.Show_Button_Hit();
                    Animator.SetTrigger("DrawIn");
                }
                if (WeaponAttack != null)
                {
                    if (!WeaponsonHands)
                    {
                        AudioManager.instance.Play_PutOnWeapon();
                        WeaponsonHands = true;
                    }
                }
            }
            if (WeaponDefend != null)
            {
                if(!ShieldonHands)
                {
                    StartCoroutine(PutOnShield());
                    GameCanvas_Controller.instance.Show_Button_Shield();
                }
                if (WeaponDefend != null)
                {
                    if (!ShieldonHands)
                    {
                        AudioManager.instance.Play_PutOnWeapon();
                        ShieldonHands = true;
                    }
                }
            }
            
        }

        public void Lock(bool decision)
        {
            characterController.enabled = decision;
            thirdPersonController.enabled = decision;
        }

        public void TaketheWeapon()
        {
            WeaponScript newWeapon = InteractedItem.GetComponent<WeaponScript>();
            if (newWeapon.isTaken == false)
            {
                if (newWeapon.weaponType == WeaponType.Shield)
                {
                    newWeapon.transform.parent = bodyPart.WeaponDefendPutOnParent;
                    newWeapon.transform.localPosition = bodyPart.WeaponDefendPutOnTransform.localPosition;
                    HideInteractSprite();
                    newWeapon.transform.localRotation = bodyPart.WeaponDefendPutOnTransform.localRotation;
                    newWeapon.transform.localScale = bodyPart.WeaponDefendPutOnTransform.localScale;
                    WeaponDefend = InteractedItem;
                    PlayerPrefs.SetInt("WeaponDefend", newWeapon.ID);
                }
                else if (newWeapon.weaponType == WeaponType.Axe)
                {
                    if (WeaponAttack != null)
                    {
                        WeaponAttack.transform.parent = newWeapon.transform.parent;
                        WeaponAttack.GetComponent<WeaponScript>().isTaken = false;
                        WeaponAttack.GetComponent<WeaponScript>().colliderGrab.enabled = true;
                        WeaponAttack.transform.localPosition = newWeapon.transform.localPosition;
                        WeaponAttack.transform.localRotation = newWeapon.transform.localRotation;
                    }
                    newWeapon.transform.parent = bodyPart.WeaponAttackPutOnParent;
                    newWeapon.transform.localPosition = bodyPart.WeaponAttackPutOnTransform.localPosition;
                    HideInteractSprite();
                    newWeapon.transform.localRotation = bodyPart.WeaponAttackPutOnTransform.localRotation;
                    WeaponAttack = InteractedItem;
                    PlayerPrefs.SetInt("WeaponAttack", newWeapon.ID);
                }
                else if (newWeapon.weaponType == WeaponType.Sword)
                {
                    if (WeaponAttack != null)
                    {
                        WeaponAttack.transform.parent = newWeapon.transform.parent;
                        WeaponAttack.GetComponent<WeaponScript>().isTaken = false;
                        WeaponAttack.GetComponent<WeaponScript>().colliderGrab.enabled = true;
                        WeaponAttack.transform.localPosition = newWeapon.transform.localPosition;
                        WeaponAttack.transform.localRotation = newWeapon.transform.localRotation;
                    }
                    newWeapon.transform.parent = bodyPart.WeaponAttackPutOnParent;
                    newWeapon.transform.localPosition = bodyPart.WeaponAttackPutOnTransform.localPosition;
                    HideInteractSprite();
                    newWeapon.transform.localRotation = bodyPart.WeaponAttackPutOnTransform.localRotation;
                    WeaponAttack = InteractedItem;
                    PlayerPrefs.SetInt("WeaponAttack", newWeapon.ID);
                }

                newWeapon.isTaken = true;
                GameCanvas_Controller.instance.Show_Grabbed_Text(newWeapon.GetComponent<WeaponScript>().Name);
                newWeapon.colliderGrab.enabled = false;
                AudioManager.instance.Play_Grab();
            }
            GetActionOnWeapon();
            InteractedItem = null;
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Door"))
            {
                InteractedItemType = ItemType.Door;
                InteractedItem = other.gameObject;
                ShowInteractSprite();
            }
            else if (other.CompareTag("Weapon") && !other.GetComponent<WeaponScript>().isTaken)
            {
                InteractedItemType = ItemType.Weapon;
                InteractedItem = other.gameObject;
                ShowInteractSprite();
            }
            else if (other.CompareTag("Item"))
            {
                if (other.GetComponent<PotionScript>() != null)
                {
                    InteractedItemType = ItemType.Potion;
                }
                else if (other.GetComponent<KeyScript>() != null)
                {
                    InteractedItemType = ItemType.Key;
                }
                if (other.GetComponent<ChestScript>() != null)
                {
                    InteractedItemType = ItemType.Chest;
                }
                if (other.GetComponent<AsansorScript>() != null)
                {
                    InteractedItemType = ItemType.Elevator;
                }
                if (other.GetComponent<AynaCarkScript>() != null)
                {
                    InteractedItemType = ItemType.Mirror;
                }
                InteractedItem = other.gameObject;
                ShowInteractSprite();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Item"))
            {
                if (InteractedItemType == ItemType.Key || InteractedItemType == ItemType.Potion || InteractedItemType == ItemType.Chest || InteractedItemType == ItemType.Elevator || InteractedItemType == ItemType.Mirror)
                {
                    InteractedItem = null;
                    HideInteractSprite();
                    InteractedItemType = ItemType.None;
                }
            }
            else if (other.CompareTag("Door"))
            {
                if (InteractedItemType == ItemType.Door)
                {
                    InteractedItem = null;
                    HideInteractSprite();
                    InteractedItemType = ItemType.None;
                }
            }
            else if (other.CompareTag("Weapon"))
            {
                if (InteractedItemType == ItemType.Weapon)
                {
                    InteractedItem = null;
                    HideInteractSprite();
                    InteractedItemType = ItemType.None;
                }
            }
        }

        public void ResetCollider()
        {
            cold.enabled = false;
            cold.enabled = true;
        }

        private bool gettingHit = false;
        IEnumerator ResetGetHit()
        {
            gettingHit = true;
            thirdPersonController.MoveSpeed = 0;
            thirdPersonController.SprintSpeed = 0;
            yield return new WaitForSeconds(0.25f);
            thirdPersonController.MoveSpeed = Speed;
            thirdPersonController.SprintSpeed = SprintSpeed;
            gettingHit = false;
        }

        public void AddHealth(int adding)
        {
            Health = Health + adding;
            if (Health > TotalHealth)
            {
                Health = (int)TotalHealth;
            }
            GameCanvas_Controller.instance.Update_Health_Bar();
        }


        public void AddMana(int adding)
        {
            Mana = Mana + adding;
            if (Mana > TotalMana)
            {
                Mana = (int)TotalMana;
            }
            GameCanvas_Controller.instance.Update_Mana_Bar(0);
        }


        IEnumerator AddSpeed(int extraSpeed)
        {
            Speed = Speed + extraSpeed;
            thirdPersonController.MoveSpeed = thirdPersonController.MoveSpeed + extraSpeed;
            thirdPersonController.SprintSpeed = thirdPersonController.SprintSpeed + extraSpeed;
            yield return new WaitForSeconds(30);
            Speed = Speed - extraSpeed;
            thirdPersonController.MoveSpeed = thirdPersonController.MoveSpeed - extraSpeed;
            thirdPersonController.SprintSpeed = thirdPersonController.SprintSpeed - extraSpeed;
        }

        IEnumerator AddShield(int time)
        {
            BodyShield.SetActive(true);
            yield return new WaitForSeconds(time);
            BodyShield.SetActive(false);
        }

        IEnumerator AddInvisibility(int time)
        {
            isInvisible = true;
            var meshRenderers = transform.GetComponentsInChildren<SkinnedMeshRenderer>();
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                meshRenderers[i].enabled = false;
                meshRenderers[i].tag = "Untagged";
            }
            var meshRenderers2 = transform.GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < meshRenderers2.Length; i++)
            {
                meshRenderers2[i].enabled = false;
                meshRenderers2[i].tag = "Untagged";
            }
            yield return new WaitForSeconds(time);
            isInvisible = false;
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                meshRenderers[i].enabled = true;
                meshRenderers[i].tag = "Player";
            }
            for (int i = 0; i < meshRenderers2.Length; i++)
            {
                meshRenderers2[i].enabled = true;
                meshRenderers2[i].tag = "Player";
            }
        }

        public void GetDamagePlayer(int damage)
        {
            if (gettingHit) return;
            if (BodyShield.activeSelf) return;
            if (Health <= 0)
            {
                return;
            }
            if (inDefendMode)
            {
                //Animator.SetTrigger("GetImpact");
                AudioManager.instance.Play_ShieldImpact();
                return;
            }
            Health = Health - damage;
            if (Health <= 0)
            {
                Health = 0;
            }
            Animator.SetInteger("Health", Health);
            if (!isHitting)
            {
                StartCoroutine(ResetGetHit());
            }
            if (Health == 0)
            {
                AudioManager.instance.Play_Die();
                GetComponent<CharacterController>().enabled = false;
                GetComponent<ThirdPersonController>().enabled = false;
                Animator.SetTrigger("Die");
                StartCoroutine(GameCanvas_Controller.instance.Show_GameOver_Panel());
            }
            GameCanvas_Controller.instance.Update_Health_Bar();
            GameCanvas_Controller.instance.Show_Damage_Effect();
            AdvancedTPSCamera.Instance.fCamShakeImpulse = 0.3f;
        }

        public void UpdateCoin(int amount)
        {
            Coin += amount;
            PlayerPrefs.SetInt("Coin", Coin);
            AudioManager.instance.Play_Coin();
            GameCanvas_Controller.instance.Update_Text_Coin();
        }
    }

    public enum ItemType
    {
        Weapon,
        Key,
        Door,
        Potion,
        None,
        Chest,
        Elevator,
        Button,
        Mirror
    }
}