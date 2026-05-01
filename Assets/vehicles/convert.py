import re
import csv

raw_path = r"C:\Users\elias\source\projects\MagicOGK-OIV-Builder-master-v1\Assets\vehicles\vehicles_raw.txt"
out_path = r"C:\Users\elias\source\projects\MagicOGK-OIV-Builder-master-v1\Assets\vehicles\vehicles.csv"

text = open(raw_path, "r", encoding="utf-8").read()

pattern = re.compile(
    r"Display Name:\s*(.*?)\s+Hash:\s*[-\d]+\s+Model Name:\s*([a-zA-Z0-9_]+)",
    re.S
)

rows = []
seen = set()

for display, model in pattern.findall(text):
    display = " ".join(display.split()).strip()
    model = model.strip().lower()

    key = (display, model)
    if key in seen:
        continue

    seen.add(key)
    rows.append((display, model, model))

with open(out_path, "w", encoding="utf-8", newline="") as f:
    writer = csv.writer(f)
    writer.writerow(["DisplayName", "SpawnName", "ImageName"])
    writer.writerows(rows)

print(f"Created {out_path} with {len(rows)} vehicles")