using UnityEngine;

namespace liminal
{
    public class DebugController : MonoBehaviour
    {
        DebugInput _debugInput;
        int _currentCharacter = -1;

        [SerializeField] LiminalPlayerCharacterController[] _characters;

        void Awake()
        {
            _debugInput = new();

            for (var i =0; i < _characters.Length; i++)
            {
                var character = _characters[i];
                if (character.IsLocalPlayer)
                {
                    _currentCharacter = i;
                    break;
                }
            }
            
            if (_currentCharacter == -1)
                Debug.LogWarning("No characters are selected as the local player");
        }

        void OnEnable()
        {
            _debugInput.Enable();
        }

        void OnDisable()
        {
            _debugInput.Disable();
        }

        void Update()
        {
            if (_debugInput.Default.SwitchCharacters.WasReleasedThisFrame())
            {
                if (_currentCharacter >= 0)
                    _characters[_currentCharacter].IsLocalPlayer = false;

                _currentCharacter++;
                if (_currentCharacter >= _characters.Length)
                    _currentCharacter = 0;

                _characters[_currentCharacter].IsLocalPlayer = true;
            }
        }
    }
}
