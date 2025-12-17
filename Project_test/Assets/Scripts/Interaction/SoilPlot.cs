using UnityEngine;

public class SoilPlot : MonoBehaviour
{
    [Header("Plante à créer quand une graine est plantée")]
    public PlantGrowth plantPrefab;

    private PlantGrowth plantedPlant;
    private bool hasBeenWatered = false;

    private void OnTriggerEnter(Collider other)
    {
        // 1) Plantation : une graine tombe sur la terre
        if (other.CompareTag("Seed") && plantedPlant == null)
        {
            PlantSeed(other.gameObject);
        }
    }

    private void PlantSeed(GameObject seed)
    {
        // On enlève la graine physique
        Destroy(seed);

        // On instancie la plante juste au-dessus du sol
        Vector3 spawnPos = transform.position;
        spawnPos.y += 0.01f;

        plantedPlant = Instantiate(plantPrefab, spawnPos, Quaternion.identity);

        // On affiche seulement la graine de la plante
        plantedPlant.ShowSeed();

        // 👉 Afficher le message "Voulez-vous arroser ?"
        WaterPromptUI.Instance.Show(
            this,
            transform.position + Vector3.up * 0.3f
        );
    }

    /// Appelé par le bouton "Oui"
    public void WaterPlant()
    {
        if (plantedPlant == null || hasBeenWatered) return;

        hasBeenWatered = true;
        plantedPlant.StartGrowth();
    }
}
