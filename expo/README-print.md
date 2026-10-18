# NMU Shuttle — Expo Posters

Print-ready A3 posters for the expo showcase.

| File | What it shows | Audience |
|------|---------------|----------|
| `poster-1-product.html` | Product overview, key features, trip flow, user roles | Passersby / general |
| `poster-2-tech.html` | Architecture diagram, tech stack, deployment | Lecturers / judges |
| `poster-3-student.html` | "Experience LATT Mobile" — scannable QR + app screenshots | Students (try it now) |

All are **A3 portrait** (297 × 420 mm), use the university brand colours
(navy `#132e51`, gold `#ffcc00`) and the Hanken Grotesk font.

## A4 versions

A4 copies of every poster live in `a4/`:

- `a4/poster-1-product-A4.html`
- `a4/poster-2-tech-A4.html`
- `a4/poster-3-student-A4.html`

They reuse the exact A3 layout scaled to fit A4 (both share the same 1:√2 ratio,
so nothing reflows). Print them the same way, just pick **A4** as the paper size.

## PNG exports

Ready-to-use PNGs are in `png/` (rendered at 3× for sharp print):

| File | Size | Pixels |
|------|------|--------|
| `png/poster-1-product-A3.png` | A3 | 3369 × 4761 |
| `png/poster-2-tech-A3.png` | A3 | 3369 × 4761 |
| `png/poster-3-student-A3.png` | A3 | 3369 × 4761 |
| `png/poster-1-product-A4.png` | A4 | 2382 × 3369 |
| `png/poster-2-tech-A4.png` | A4 | 2382 × 3369 |
| `png/poster-3-student-A4.png` | A4 | 2382 × 3369 |

Use the PNGs for quick sharing, slides, or print shops that prefer images.
For the crispest print, the PDF export (below) is still best since it stays vector.

### Regenerating the PNGs

If you edit a poster, re-render with:

```bash
cd expo
node render.mjs
```

This uses Playwright driving your installed Google Chrome (no browser download).
If Chrome is somewhere else, edit the `executablePath` near the top of `render.mjs`.

## Adding app screenshots (poster 3)

Poster 3 shows two phone mockups — a **Student** and a **Marshal** screen.
Until you add real images, each phone shows a tidy placeholder, so the poster
is safe to show as-is. To use real screenshots:

1. Take a screenshot in each app (a portrait phone screen works best).
2. Save them into the `screenshots/` folder next to the posters, named exactly:
   - `screenshots/student.jpg`
   - `screenshots/marshal.jpg`
3. Reopen `poster-3-student.html` — the images fill the phones automatically.

Tips:
- Portrait images around **1080 × 2160 px** (or any tall phone ratio) look best;
  they are cropped to fill the frame.
- The poster is wired to `.jpg` files. If you swap to PNG, update the file names in
  the `<img src="...">` tags inside `poster-3-student.html` to match.

### The QR code

The QR on poster 3 is a real, scannable code pointing at the live student app:
**`https://noormosia.github.io/NMUShuttles/app/`**. It is embedded directly in the
HTML (vector), so it prints razor-sharp and needs no internet. A standalone copy
also lives at `screenshots/qr-student-app.svg`. To point it somewhere else,
regenerate it with:

```bash
npx qrcode "https://your-new-url" -t svg -o screenshots/qr-student-app.svg -w 600 -m 2
```

then paste the new `<path stroke=...>` data into the `<svg>` block in `poster-3-student.html`.

## Export to print-ready PDF

1. Open the `.html` file in **Google Chrome** or **Microsoft Edge** (best print engine).
   - Internet is needed the first time so fonts/icons load from Google Fonts.
2. Press **Ctrl + P**.
3. Set these options:
   - **Destination:** Save as PDF
   - **Paper size:** A3
   - **Layout:** Portrait
   - **Margins:** None
   - **Scale:** Default (100%) — do not "Fit to page"
   - **Options:** tick **Background graphics** (critical — otherwise the navy/gold fills disappear)
4. Save. You now have a vector-sharp A3 PDF to send to the printer.

## Printing tips

- Ask the print shop for **A3, full colour, 150–200 gsm** (a little card stock so it stands up).
- If you want edge-to-edge colour, ask for **full bleed** — the navy header and gold strip run to the page edge.
- Matte finish reduces glare under expo lights.

## Editing

Everything is plain HTML/CSS — open in any editor and change the text directly.
Colours are defined once at the top of each file in the `:root` block, so you can
retheme both posters by editing those variables.

## Quick preview

Just double-click any `.html` file to open it in your browser. What you see is
what prints (minus the grey page background, which is screen-only).
