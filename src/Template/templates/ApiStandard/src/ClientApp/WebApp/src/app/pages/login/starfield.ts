// 动态星空背景，支持多色漂移和闪烁
// 物理立体感星点结构，增加 depth（距离）
interface Star {
  x: number;
  y: number;
  r: number; // 半径
  color: string;
  dx: number;
  dy: number;
  twinkle: number;
  twinkleSpeed: number;
  depth: number; // 0=最远, 1=最近
}

// 星点颜色可配置，默认仅白、黄、蓝
export let STAR_COLORS = [
  'rgba(255,255,255,0.85)', // 白
  'rgba(255,220,120,0.85)', // 黄
  'rgba(120,180,255,0.85)', // 蓝
];

export function setStarColors(colors: string[]) {
  STAR_COLORS = colors;
}

function random(min: number, max: number) {
  return Math.random() * (max - min) + min;
}

// 生成带距离感的星点
// 生成带距离感的星点，颜色可配置
function createStars(count: number, w: number, h: number): Star[] {
  const stars: Star[] = [];
  for (let i = 0; i < count; i++) {
    // depth: 0=最远, 1=最近，近的星点更大更亮更快
    const depth = Math.pow(random(0, 1), 2.2); // 趋向远距离分布
    const color = STAR_COLORS.length > 0
      ? STAR_COLORS[Math.floor(Math.random() * STAR_COLORS.length)]
      : 'rgba(255,255,255,0.85)';
    // 物理参数映射
    const r = 0.7 + depth * 2.3; // 半径 0.7~3.0
    const brightness = 0.5 + depth * 0.5; // 亮度 0.5~1.0
    const dx = (random(-0.03, 0.03)) * (0.3 + depth * 1.2); // 远的慢，近的快
    const dy = (random(-0.02, 0.02)) * (0.3 + depth * 1.2);
    const twinkle = random(0.7, 1) * brightness;
    const twinkleSpeed = random(0.002, 0.008) * (0.5 + depth);
    stars.push({
      x: random(0, w),
      y: random(0, h),
      r,
      color,
      dx,
      dy,
      twinkle,
      twinkleSpeed,
      depth
    });
  }
  return stars;
}

export function initStarfield(canvas: HTMLCanvasElement) {
  const ctx = canvas.getContext('2d');
  if (!ctx) return;
  let w = canvas.width = canvas.offsetWidth;
  let h = canvas.height = canvas.offsetHeight;
  let stars = createStars(w > 900 ? 80 : 40, w, h);

  function resize() {
    w = canvas.width = canvas.offsetWidth;
    h = canvas.height = canvas.offsetHeight;
    stars = createStars(w > 900 ? 80 : 40, w, h);
  }
  window.addEventListener('resize', resize);

  function draw() {
    if (!ctx) return;
    ctx.clearRect(0, 0, w, h);
    for (const star of stars) {
      // 物理感：近的星点更大更亮，远的更小更暗
      star.x += star.dx;
      star.y += star.dy;
      // 循环边界
      if (star.x < 0) star.x += w;
      if (star.x > w) star.x -= w;
      if (star.y < 0) star.y += h;
      if (star.y > h) star.y -= h;
      // twinkle动画
      star.twinkle += Math.sin(Date.now() * star.twinkleSpeed) * 0.01;
      // 亮度与距离相关
      const alpha = Math.max(0.3, 0.7 * star.depth + 0.3) * star.twinkle;
      ctx.save();
      ctx.beginPath();
      ctx.arc(star.x, star.y, star.r, 0, Math.PI * 2);
      ctx.globalAlpha = alpha;
      ctx.fillStyle = star.color;
      ctx.shadowColor = star.color;
      ctx.shadowBlur = 2 + star.depth * 6;
      ctx.fill();
      ctx.restore();
    }
    requestAnimationFrame(draw);
  }
  draw();
}
