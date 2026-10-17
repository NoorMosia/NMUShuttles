// One-off icon generator.
// Renders assets/logo.svg into favicons + PWA install icons for both web apps.
//
// Usage (from tools/gen-icons):
//   npm install
//   node build-icons.mjs
//
// Produces, for each target web/ folder:
//   favicon.png                (logo, transparent)
//   icons/Icon-192.png         (logo on navy, "any" purpose)
//   icons/Icon-512.png
//   icons/Icon-maskable-192.png (extra safe-zone padding for Android masking)
//   icons/Icon-maskable-512.png
// Plus copies logo.svg -> each web/favicon.svg.

import { readFileSync, writeFileSync, mkdirSync, copyFileSync } from 'node:fs';
import { dirname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import sharp from 'sharp';

const __dirname = dirname(fileURLToPath(import.meta.url));
const repoRoot = resolve(__dirname, '..', '..');

const NAVY = '#132e51';
const LOGO_SVG = join(repoRoot, 'assets', 'logo.svg');

const rawLogo = readFileSync(LOGO_SVG, 'utf8');

// Recolor the black logo to a given fill (used for the light-on-navy install icon).
function recolor(svg, hex) {
  return svg.replaceAll('fill="#000000"', `fill="${hex}"`);
}

// Build a square icon SVG: solid background + the logo scaled into a centered
// box that leaves `padFraction` of the canvas as margin on each side.
// For maskable icons Android crops to a circle/squircle, so more padding keeps
// the logo inside the "safe zone".
function iconSvg({ size, bg, logoFill, padFraction }) {
  const inner = Math.round(size * (1 - padFraction * 2));
  const offset = Math.round((size - inner) / 2);
  // Embed the logo as a nested <svg> so its 1024x1024 viewBox scales to `inner`.
  const logoBody = recolor(rawLogo, logoFill)
    // strip the outer <svg ...> wrapper's width/x/y so we can re-wrap cleanly
    .replace(/<\?xml[^>]*\?>/g, '')
    .replace(/<svg[^>]*>/, '')
    .replace(/<\/svg>\s*$/, '');
  return `<svg xmlns="http://www.w3.org/2000/svg" width="${size}" height="${size}" viewBox="0 0 ${size} ${size}">
  <rect width="${size}" height="${size}" rx="${Math.round(size * 0.18)}" fill="${bg}"/>
  <svg x="${offset}" y="${offset}" width="${inner}" height="${inner}" viewBox="0 0 1024 1024">${logoBody}</svg>
</svg>`;
}

// Maskable icons should be a full-bleed square (no rounded corners — the OS
// applies the mask), so use rx=0 there.
function maskableSvg({ size, bg, logoFill, padFraction }) {
  return iconSvg({ size, bg, logoFill, padFraction })
    .replace(`rx="${Math.round(size * 0.18)}"`, 'rx="0"');
}

async function renderPng(svg, outPath, size) {
  await sharp(Buffer.from(svg)).resize(size, size).png().toFile(outPath);
  console.log('wrote', outPath);
}

async function buildFor(webDir) {
  const iconsDir = join(webDir, 'icons');
  mkdirSync(iconsDir, { recursive: true });

  // favicon.svg — the plain logo, crisp at any size.
  copyFileSync(LOGO_SVG, join(webDir, 'favicon.svg'));
  console.log('wrote', join(webDir, 'favicon.svg'));

  // favicon.png — plain logo on transparent, small raster fallback.
  const faviconSvg = `<svg xmlns="http://www.w3.org/2000/svg" width="256" height="256" viewBox="0 0 1024 1024">${rawLogo.replace(/<\?xml[^>]*\?>/g,'').replace(/<svg[^>]*>/, '').replace(/<\/svg>\s*$/, '')}</svg>`;
  await renderPng(faviconSvg, join(webDir, 'favicon.png'), 256);

  // Standard ("any") install icons: white logo on navy, modest padding.
  for (const size of [192, 512]) {
    const svg = iconSvg({ size, bg: NAVY, logoFill: '#ffffff', padFraction: 0.12 });
    await renderPng(svg, join(iconsDir, `Icon-${size}.png`), size);
  }

  // Maskable install icons: more padding (safe zone ~20%).
  for (const size of [192, 512]) {
    const svg = maskableSvg({ size, bg: NAVY, logoFill: '#ffffff', padFraction: 0.22 });
    await renderPng(svg, join(iconsDir, `Icon-maskable-${size}.png`), size);
  }
}

const targets = [
  join(repoRoot, 'AppUIWeb', 'web'),
  join(repoRoot, 'WebPortalUI', 'web'),
];

for (const t of targets) {
  console.log('--- building icons for', t, '---');
  await buildFor(t);
}
console.log('done');
