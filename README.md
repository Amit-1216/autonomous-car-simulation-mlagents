# Autonomous Car Simulation using Reinforcement Learning 🚗🤖

This project presents an **Autonomous Car Simulation** developed in **Unity** using **ML-Agents** and **Reinforcement Learning**.  
The objective is to train a virtual car to drive autonomously by learning from rewards and penalties in a simulated environment.

This project is created as part of an **academic / final-year project** to demonstrate practical applications of **Artificial Intelligence** and **Reinforcement Learning**.

---

## 📌 Features

- Autonomous car control using Reinforcement Learning
- Collision avoidance with wall penalty system
- Reward–penalty based learning system
- Real-time HUD (Speed, Steering Angle, Reward, Lap Count)
- Lap counting via finish line trigger
- Manual driving mode for testing (Human control)
- Scripted demo mode
- Car reset system (press **R** in Human mode)
- Lane following behavior
- Collision avoidance
- Reward–penalty based learning system
- Real-time simulation in Unity

- Configurable training parameters using YAML

---

## 🧠 Reinforcement Learning Approach

The agent (car) learns through **trial and error** by interacting with the environment.

### ✔ Actions (Continuous)
| Index | Action | Range |
|-------|--------|-------|
| 0 | Steering (left / right) | -1 to 1 |
| 1 | Throttle (accelerate) | -1 to 1 |
| 2 | Brake | 0 to 1 |

> Dead zones applied: values below ±0.05 are clamped to 0

### 🎯 Observations (VectorSensor)
| # | Observation | Description |
|---|-------------|-------------|
| 1 | Speed | `velocity.magnitude / 20f` |
| 2 | Local velocity X | Lateral speed normalized |
| 3 | Local velocity Z | Forward speed normalized |
| 4–8 | Ray distances | 5 raycast distances to walls |

### 🏆 Reward System
| Condition | Reward |
|-----------|--------|
| Moving forward | `+forwardSpeed × 0.0015` |
| Lateral sliding | `-abs(lateralSpeed) × 0.002` |
| Time penalty (per step) | `-0.0005` |
| Wall collision | `-2.0` + EndEpisode |

---

## 🚗 Car Physics

The `CarController` features realistic simulation-grade physics:

- **Drivetrain modes** — Front Wheel, Rear Wheel, All Wheel Drive
- **Traction Control** — cuts torque on wheel slip exceeding threshold
- **ABS** — reduces brake force during wheel lock-up
- **Anti-Roll Bars** — counters body roll during cornering
- **Speed-sensitive steering** — reduces steer angle at high speed
- **Engine braking** — applied when throttle is released
- **Handbrake** — triggers rear wheel drift behavior
=======
### ✔ Actions
- Steering (left / right)
- Acceleration
- Braking

### 🎯 Rewards
- Positive reward for moving forward
- Positive reward for staying on the track
- Negative reward for collisions
- Negative reward for going off-road
- Episode ends on major collision

This reward system helps the agent gradually learn **safe and efficient driving behavior**.

---

## 🛠 Tech Stack

- **Unity Engine** (2022+)
- **Unity ML-Agents** (`com.unity.ml-agents`)
- **C#** (Agent, Controller, HUD, Lap logic)
- **Python** (Training via `mlagents-learn`)
- **Unity Engine**
- **Unity ML-Agents**
- **C#** (Agent & environment logic)
- **Python** (Training)

- **YAML** (Training configuration)

---

## 📂 Project Structure

```text
demo/
├── Assets/
│   ├── Scripts/
│   │   ├── CarController.cs     ← Physics, input, reset logic
│   │   ├── CarAgent.cs          ← ML-Agents brain, rewards
│   │   ├── CarState.cs          ← Raycast sensors
│   │   ├── CarHUD.cs            ← OnGUI HUD (speed, steer, reward, lap)
│   │   └── LapTrigger.cs        ← Finish line lap counter
│   └── Scenes/

├── Packages/
├── ProjectSettings/
├── car_config.yaml
├── README.md
└── .gitignore

```

---

## ▶️ How to Run the Project

### 1️⃣ Open in Unity
1. Open **Unity Hub**
2. Click **Open Project**
3. Select the `demo` folder
4. Install **ML-Agents** package via `Window → Package Manager → Add by name: com.unity.ml-agents`

### 2️⃣ Run in Human Mode (Manual Driving)
1. Select the car in the Hierarchy
2. Set **Control Mode → Human** in the `CarController` Inspector
3. Press **Play**

| Key | Action |
|-----|--------|
| Arrow Keys / WASD | Drive |
| Space | Brake |
| Left Shift | Handbrake |
| R | Reset car to start position |

### 3️⃣ Run in ML-Agent Mode (Inference)
1. Set **Control Mode → MLAgent**
2. Attach a trained `.onnx` model to the **Behavior Parameters → Model** slot
3. Press **Play**

---

## 🏋️ Training the Agent

Make sure Python and ML-Agents are installed:

```bash
pip install mlagents
```

Start training:

```bash
mlagents-learn car_config.yaml --run-id=CarRun01
```

Then press **Play** in Unity Editor to begin the training session.  
Trained models are saved to `results/CarRun01/`.

---

## 🖥️ HUD Display

A live in-game HUD renders in the top-left corner during play:

```
Speed:    72 km/h
Steering: -4.21°
Reward:   12.6
Lap:      2
```

No Canvas setup required — rendered via Unity's `OnGUI()` system.

---

## 🔧 Scene Setup Checklist

- [ ] Car GameObject tagged as **Player**
- [ ] Wall/barrier objects assigned to **Wall** layer
- [ ] `CarState → Obstacle Layers` set to **Wall**
- [ ] `LapTrigger` Box Collider has **Is Trigger** enabled
- [ ] `CarHUD` attached to car and `CarController` slot filled

---

## 📈 Future Improvements

- Traffic signal handling
- Multiple autonomous vehicles (multi-agent system)
- Improved reward shaping with track centerline data
- Lap time tracking and best lap display
- Integration with real-world driving datasets

---

## 👨‍💻 Author

**Amit Maurya**  
Autonomous Car Simulation using Unity ML-Agents

---

## 📜 License

This project is intended for educational and academic use.
=======

▶️ How to Run the Project
1️⃣ Open in Unity

Open Unity Hub
Click Open Project
Select the demo folder

2️⃣ Run Simulation

Press Play in Unity Editor

## Training the Agent

Make sure Python and ml-agents are installed.
mlagents-learn car_config.yaml
Then press Play in Unity to start training.

📈 Future Improvements

Traffic signal handling
Multiple autonomous vehicles (multi-agent system)
Sensor-based perception (raycasts / lidar simulation)
Integration with real-world driving datasets
Improved reward shaping

👨‍💻 Author

Amit Maurya
Autonomous Car Simulation using Unity ML-Agents

📜 License

This project is intended for educational and academic use.
