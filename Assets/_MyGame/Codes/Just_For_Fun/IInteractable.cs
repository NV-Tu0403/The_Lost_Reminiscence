public interface IInteractable
{
    // Bất kỳ vật thể nào muốn tương tác đều phải có hàm này
    void Interact(PlayerPuzzleInteractor interactor);
}