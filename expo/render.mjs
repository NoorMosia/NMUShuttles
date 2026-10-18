// Renders the poster HTML files to high-resolution PNGs using headless Chromium.
// Usage: node render.mjs
import { chromium } from 'playwright';
import { fileURLToPath, pathToFileURL } from 'url';
import { dirname, join } from 'path';

const here = dirname(fileURLToPath(import.meta.url));

// A3 portrait = 297 x 420 mm. At 96 CSS px per inch:
//   297mm = 1122.52px, 420mm = 1587.40px
const A3 = { w: 1123, h: 1587 };
// A4 portrait = 210 x 297 mm -> 793.70 x 1122.52 px
const A4 = { w: 794, h: 1123 };

const SCALE = 3; // device scale factor -> ~3x pixel density for print sharpness

const jobs = [
  // A3 originals
  { file: 'poster-1-product.html',  out: 'png/poster-1-product-A3.png',  size: A3 },
  { file: 'poster-2-tech.html',     out: 'png/poster-2-tech-A3.png',     size: A3 },
  { file: 'poster-3-student.html',  out: 'png/poster-3-student-A3.png',  size: A3 },
  // A4 versions
  { file: 'a4/poster-1-product-A4.html', out: 'png/poster-1-product-A4.png', size: A4 },
  { file: 'a4/poster-2-tech-A4.html',    out: 'png/poster-2-tech-A4.png',    size: A4 },
  { file: 'a4/poster-3-student-A4.html', out: 'png/poster-3-student-A4.png', size: A4 },
];

// Use the system-installed Chrome so no Playwright browser download is needed.
const browser = await chromium.launch({
  executablePath: 'C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe',
});

for (const job of jobs) {
  const page = await browser.newPage({
    viewport: { width: job.size.w, height: job.size.h },
    deviceScaleFactor: SCALE,
  });
  const url = pathToFileURL(join(here, job.file)).href;
  await page.goto(url, { waitUntil: 'networkidle' });
  // Give web fonts a beat to settle so text is not rendered in a fallback face.
  await page.evaluate(() => document.fonts.ready);
  await page.waitForTimeout(400);
  await page.screenshot({
    path: join(here, job.out),
    clip: { x: 0, y: 0, width: job.size.w, height: job.size.h },
  });
  console.log('rendered', job.out);
  await page.close();
}

await browser.close();
console.log('done');
