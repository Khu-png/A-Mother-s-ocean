const LEVEL_NUMBER = 4;
const LEVEL_TIME_SECONDS = 120;
const REWARD_COINS = 20;
const INITIAL_COINS = 120;
const GRID_ROWS = ["S....", ".T...", ".....", "...T.", "....F"];

const homeScreen = document.querySelector("#homeScreen");
const gameplayScreen = document.querySelector("#gameplayScreen");
const boardElement = document.querySelector("#board");
const timerElement = document.querySelector("#timer");
const targetCountElement = document.querySelector("#targetCount");
const pauseOverlay = document.querySelector("#pauseOverlay");
const loseOverlay = document.querySelector("#loseOverlay");
const winOverlay = document.querySelector("#winOverlay");
const confettiCanvas = document.querySelector("#confetti");
const coinFlights = document.querySelector("#coinFlights");
const gameShell = document.querySelector(".device");

let coins = INITIAL_COINS;
let levelNumber = LEVEL_NUMBER;
let secondsLeft = LEVEL_TIME_SECONDS;
let timerHandle = 0;
let position = { row: 0, column: 0 };
let collected = new Set();
let visited = new Set(["0,0"]);
let gameActive = false;
let isPaused = false;
let hasClaimed = false;
let pointerStart = null;
let confettiPieces = [];
let confettiFrame = 0;

function showScreen(screen) {
  homeScreen.classList.toggle("active", screen === "home");
  gameplayScreen.classList.toggle("active", screen === "game");
  homeScreen.setAttribute("aria-hidden", screen !== "home");
  gameplayScreen.setAttribute("aria-hidden", screen !== "game");
}

function startLevel() {
  clearInterval(timerHandle);
  position = { row: 0, column: 0 };
  collected = new Set();
  visited = new Set(["0,0"]);
  secondsLeft = LEVEL_TIME_SECONDS;
  gameActive = true;
  isPaused = false;
  hasClaimed = false;
  document.querySelector("#gameLevel").textContent = levelNumber.toString().padStart(2, "0");
  pauseOverlay.classList.remove("visible");
  loseOverlay.classList.remove("visible");
  winOverlay.classList.remove("visible");
  boardElement.classList.remove("locked");
  showScreen("game");
  renderBoard();
  refreshTimer();
  timerHandle = window.setInterval(tickTimer, 1000);
}

function tickTimer() {
  if (!gameActive || isPaused) return;
  secondsLeft = Math.max(0, secondsLeft - 1);
  refreshTimer();
  if (secondsLeft === 0) finishLevel(false);
}

function refreshTimer() {
  const minutes = Math.floor(secondsLeft / 60).toString().padStart(2, "0");
  const seconds = (secondsLeft % 60).toString().padStart(2, "0");
  timerElement.textContent = `${minutes}:${seconds}`;
  timerElement.classList.toggle("urgent", secondsLeft <= 10);
}

function renderBoard() {
  boardElement.replaceChildren();
  targetCountElement.textContent = `${collected.size}/2`;

  for (let row = 0; row < GRID_ROWS.length; row += 1) {
    for (let column = 0; column < GRID_ROWS[row].length; column += 1) {
      const key = `${row},${column}`;
      const type = GRID_ROWS[row][column];
      const tile = document.createElement("button");
      tile.type = "button";
      tile.className = "tile";
      tile.setAttribute("role", "gridcell");
      tile.setAttribute("aria-label", tileLabel(type, row, column));
      tile.dataset.row = row;
      tile.dataset.column = column;

      if (visited.has(key)) tile.classList.add("visited");
      if (type === "S") tile.classList.add("start");
      if (type === "T") {
        const isCollected = collected.has(key);
        tile.classList.add("target");
        if (isCollected) tile.classList.add("collected");
        else tile.append(createSharkIcon("baby"));
      }
      if (type === "F") {
        tile.classList.add("goal");
        const pearl = document.createElement("span");
        pearl.className = "pearl";
        pearl.textContent = "✦";
        tile.append(pearl);
      }
      if (position.row === row && position.column === column) {
        tile.classList.remove("target", "goal");
        tile.replaceChildren(createSharkIcon("mother", true));
      }

      boardElement.append(tile);
    }
  }
}

function tileLabel(type, row, column) {
  if (position.row === row && position.column === column) return "Mother shark";
  if (type === "T" && !collected.has(`${row},${column}`)) return "Baby shark to rescue";
  if (type === "F") return collected.size === 2 ? "Finish" : "Finish, rescue both baby sharks first";
  return "Ocean tile";
}

function createSharkIcon(kind, isPlayer = false) {
  const image = document.createElement("img");
  image.className = isPlayer ? "tile-icon player-icon" : "tile-icon";
  image.src = kind === "mother"
    ? "../Assets/_Game/Resources/Art/Generated/mother_shark_sprite.png"
    : "../Assets/_Game/Resources/Art/Generated/baby_shark_sprite.png";
  image.alt = "";
  return image;
}

function movePlayer(rowDelta, columnDelta) {
  if (!gameActive || isPaused || winOverlay.classList.contains("visible")) return;
  const nextRow = position.row + rowDelta;
  const nextColumn = position.column + columnDelta;
  if (nextRow < 0 || nextRow >= 5 || nextColumn < 0 || nextColumn >= 5) return;

  position = { row: nextRow, column: nextColumn };
  const key = `${nextRow},${nextColumn}`;
  visited.add(key);
  if (GRID_ROWS[nextRow][nextColumn] === "T") collected.add(key);
  renderBoard();

  if (GRID_ROWS[nextRow][nextColumn] === "F") {
    if (collected.size === 2) finishLevel(true);
    else showFinishHint();
  }
}

function showFinishHint() {
  targetCountElement.animate(
    [{ transform: "translateX(-50%) scale(1)" }, { transform: "translateX(-50%) scale(1.09)" }, { transform: "translateX(-50%) scale(1)" }],
    { duration: 450, easing: "ease-out" }
  );
  targetCountElement.title = "Rescue both baby sharks before reaching the pearl.";
}

function finishLevel(didWin) {
  if (!gameActive) return;
  gameActive = false;
  clearInterval(timerHandle);
  boardElement.classList.add("locked");
  if (didWin) {
    hasClaimed = false;
    document.querySelector("#winCoins").textContent = coins;
    winOverlay.classList.add("visible");
    winOverlay.setAttribute("aria-hidden", "false");
    window.setTimeout(startConfetti, 180);
  } else {
    loseOverlay.classList.add("visible");
    loseOverlay.setAttribute("aria-hidden", "false");
  }
}

function setPaused(paused) {
  if (!gameActive) return;
  isPaused = paused;
  pauseOverlay.classList.toggle("visible", paused);
  pauseOverlay.setAttribute("aria-hidden", String(!paused));
}

function returnToMenu() {
  clearInterval(timerHandle);
  gameActive = false;
  isPaused = false;
  winOverlay.classList.remove("visible");
  loseOverlay.classList.remove("visible");
  pauseOverlay.classList.remove("visible");
  homeScreen.querySelector("#homeCoins").textContent = coins;
  homeScreen.querySelector(".wallet-pill").setAttribute("aria-label", `${coins} coins`);
  homeScreen.querySelector("#homeLevel").textContent = levelNumber.toString().padStart(2, "0");
  showScreen("home");
}

function claimReward() {
  if (hasClaimed) return;
  hasClaimed = true;
  coins += REWARD_COINS;
  document.querySelector("#winCoins").textContent = coins;
  const claimButton = document.querySelector("#claimButton");
  claimButton.textContent = "CLAIMED!";
  claimButton.disabled = true;
  animateCoinFlight();
  window.setTimeout(() => {
    levelNumber += 1;
    returnToMenu();
    claimButton.textContent = `CLAIM ${REWARD_COINS}`;
    claimButton.disabled = false;
  }, 1450);
}

function animateCoinFlight() {
  const wallet = document.querySelector(".wallet-total").getBoundingClientRect();
  const source = document.querySelector(".reward-row").getBoundingClientRect();
  const bounds = gameShell.getBoundingClientRect();
  const startX = source.left + source.width / 2 - bounds.left;
  const startY = source.top + source.height / 2 - bounds.top;
  const endX = wallet.left + wallet.width / 2 - bounds.left;
  const endY = wallet.top + wallet.height / 2 - bounds.top;

  for (let i = 0; i < 7; i += 1) {
    const coin = document.createElement("span");
    coin.className = "coin-flight";
    coin.style.left = `${startX}px`;
    coin.style.top = `${startY}px`;
    coin.style.setProperty("--flight-x", `${endX - startX + (i - 3) * 8}px`);
    coin.style.setProperty("--flight-y", `${endY - startY + (i % 3 - 1) * 18}px`);
    coin.style.animationDelay = `${i * 65}ms`;
    coinFlights.append(coin);
    window.setTimeout(() => coin.remove(), 1500);
  }
}

function startConfetti() {
  const context = confettiCanvas.getContext("2d");
  const scale = window.devicePixelRatio || 1;
  confettiCanvas.width = gameShell.clientWidth * scale;
  confettiCanvas.height = gameShell.clientHeight * scale;
  context.setTransform(scale, 0, 0, scale, 0, 0);

  const colors = ["#ffd35b", "#58e5e9", "#ff8e83", "#ffffff", "#87aafa"];
  confettiPieces = Array.from({ length: 76 }, (_, index) => ({
    x: Math.random() * gameShell.clientWidth,
    y: gameShell.clientHeight * (0.08 + Math.random() * 0.56),
    vx: (Math.random() - 0.5) * 2.1,
    vy: 1.5 + Math.random() * 2.8,
    width: 7 + Math.random() * 8,
    height: 4 + Math.random() * 6,
    rotation: Math.random() * Math.PI,
    spin: (Math.random() - 0.5) * 0.13,
    color: colors[index % colors.length],
    life: 220 + Math.random() * 130
  }));
  cancelAnimationFrame(confettiFrame);
  drawConfetti(context);
}

function drawConfetti(context) {
  context.clearRect(0, 0, gameShell.clientWidth, gameShell.clientHeight);
  let activePieces = 0;
  const isEmitting = winOverlay.classList.contains("visible") && !hasClaimed;
  for (const piece of confettiPieces) {
    if (piece.life <= 0 || piece.y > gameShell.clientHeight + 20) {
      if (!isEmitting) continue;
      piece.x = Math.random() * gameShell.clientWidth;
      piece.y = -Math.random() * 110;
      piece.vy = 1.5 + Math.random() * 2.8;
      piece.life = 180 + Math.random() * 100;
    }
    activePieces += 1;
    piece.x += piece.vx;
    piece.y += piece.vy;
    piece.vy += 0.018;
    piece.rotation += piece.spin;
    piece.life -= 1;
    context.save();
    context.translate(piece.x, piece.y);
    context.rotate(piece.rotation);
    context.globalAlpha = Math.min(1, piece.life / 35);
    context.fillStyle = piece.color;
    context.fillRect(-piece.width / 2, -piece.height / 2, piece.width, piece.height);
    context.restore();
  }
  if (activePieces > 0 && winOverlay.classList.contains("visible")) {
    confettiFrame = requestAnimationFrame(() => drawConfetti(context));
  } else {
    context.clearRect(0, 0, gameShell.clientWidth, gameShell.clientHeight);
  }
}

document.querySelector("#playButton").addEventListener("click", startLevel);
document.querySelector("#pauseButton").addEventListener("click", () => setPaused(true));
document.querySelector("#resumeButton").addEventListener("click", () => setPaused(false));
document.querySelector("#restartButton").addEventListener("click", startLevel);
document.querySelector("#pauseRestartButton").addEventListener("click", startLevel);
document.querySelector("#homeButton").addEventListener("click", returnToMenu);
document.querySelector("#loseHomeButton").addEventListener("click", returnToMenu);
document.querySelector("#tryAgainButton").addEventListener("click", startLevel);
document.querySelector("#claimButton").addEventListener("click", claimReward);

boardElement.addEventListener("click", (event) => {
  const tile = event.target.closest(".tile");
  if (!tile) return;
  const row = Number(tile.dataset.row);
  const column = Number(tile.dataset.column);
  const rowDelta = row - position.row;
  const columnDelta = column - position.column;
  if (Math.abs(rowDelta) + Math.abs(columnDelta) === 1)
    movePlayer(Math.sign(rowDelta), Math.sign(columnDelta));
});

boardElement.addEventListener("pointerdown", (event) => {
  pointerStart = { x: event.clientX, y: event.clientY };
  boardElement.setPointerCapture(event.pointerId);
});

boardElement.addEventListener("pointerup", (event) => {
  if (!pointerStart) return;
  const deltaX = event.clientX - pointerStart.x;
  const deltaY = event.clientY - pointerStart.y;
  pointerStart = null;
  if (Math.max(Math.abs(deltaX), Math.abs(deltaY)) < 22) return;
  if (Math.abs(deltaX) > Math.abs(deltaY))
    movePlayer(0, Math.sign(deltaX));
  else
    movePlayer(Math.sign(deltaY), 0);
});

boardElement.addEventListener("pointercancel", () => {
  pointerStart = null;
});

window.addEventListener("keydown", (event) => {
  const moves = {
    ArrowUp: [-1, 0],
    w: [-1, 0],
    ArrowDown: [1, 0],
    s: [1, 0],
    ArrowLeft: [0, -1],
    a: [0, -1],
    ArrowRight: [0, 1],
    d: [0, 1]
  };
  const move = moves[event.key];
  if (move && gameplayScreen.classList.contains("active")) {
    event.preventDefault();
    movePlayer(move[0], move[1]);
  }
  if (event.key === "Escape" && gameplayScreen.classList.contains("active")) {
    if (isPaused) setPaused(false);
    else setPaused(true);
  }
});
