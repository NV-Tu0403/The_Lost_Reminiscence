using FMODUnity;
using UnityEngine;
using FMOD.Studio;
// Thư viện FMOD

namespace _MyGame.Codes.Props.Piano_Key
{
    public class PianoKey : MonoBehaviour
    {
        [SerializeField] private EventReference pianoEvent; // Event FMOD PianoKey
        [SerializeField] private int noteIndex; // Giá trị 0–6 tương ứng từng note
        
        public Material defaultMateral; // Material của phím đàn
        public Material highlightMaterial; // Material khi phím đàn được nhấn

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                // Tạo instance
                EventInstance instance = RuntimeManager.CreateInstance(pianoEvent);

                // Truyền vị trí 3D của phím đàn để FMOD tính spatial
                instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));

                // Set parameter PianoKey
                instance.setParameterByName("PianoKey", noteIndex);

                // Phát âm thanh
                instance.start();
                instance.release();
                
                // Thay đổi material của phím đàn
                GetComponent<Renderer>().material = highlightMaterial;
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                // Trả về material mặc định
                GetComponent<Renderer>().material = defaultMateral;
            }
        }
    }
}
