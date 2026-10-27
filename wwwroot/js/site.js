// IDMH — etkileşimler ve kaydırma animasyonları
(() => {
  const root = document.documentElement;
  const reduced = matchMedia("(prefers-reduced-motion: reduce)").matches;
  const G = window.gsap, ST = window.ScrollTrigger;
  const motion = !reduced && G && ST;
  const fine = matchMedia("(pointer: fine)").matches;
  const $ = (s, el = document) => el.querySelector(s);
  const $$ = (s, el = document) => [...el.querySelectorAll(s)];
  root.classList.add("js");
  if (!motion) root.classList.add("reduced");

  // ---------- Menü ----------
  const menu = $("[data-menu]"), menuBtn = $("[data-menu-btn]");
  const closeMenu = () => { if (!menu || menu.hidden) return; menu.hidden = true; document.body.classList.remove("menu-open"); menuBtn.setAttribute("aria-expanded", "false"); lenis?.start(); };
  menuBtn?.addEventListener("click", () => {
    if (!menu.hidden) return closeMenu();
    menu.hidden = false; document.body.classList.add("menu-open"); menuBtn.setAttribute("aria-expanded", "true"); lenis?.stop();
    if (motion) G.from($$("nav a", menu), { yPercent: 100, opacity: 0, duration: .9, stagger: .06, ease: "expo.out" });
  });
  addEventListener("keydown", e => e.key === "Escape" && closeMenu());

  // ---------- SSS ----------
  $$(".faq-item").forEach(d => {
    const body = $(".faq-body", d);
    $("summary", d).addEventListener("click", e => {
      if (!motion) return;
      e.preventDefault();
      if (d.open) G.to(body, { height: 0, duration: .5, ease: "power3.inOut", onComplete: () => { d.open = false; body.style.height = ""; } });
      else { d.open = true; G.fromTo(body, { height: 0 }, { height: body.scrollHeight, duration: .6, ease: "power3.out", onComplete: () => body.style.height = "" }); }
    });
  });

  // İletişim formu: "Genel soru" seçilince proje alanları sönükleşir
  const projectFields = $(".project-fields");
  $$('input[name="Service"]').forEach(r => r.addEventListener("change", () => projectFields && (projectFields.style.opacity = r.value === "Genel" && r.checked ? .35 : 1)));

  if (!motion) { $("[data-preloader]")?.remove(); return; }

  G.registerPlugin(ST);
  G.defaults({ ease: "expo.out" });

  // ---------- Yumuşak kaydırma ----------
  var lenis = window.Lenis ? new Lenis({ duration: 1.25, smoothWheel: true }) : null;
  if (lenis) {
    lenis.on("scroll", ST.update);
    G.ticker.add(t => lenis.raf(t * 1000));
    G.ticker.lagSmoothing(0);
    window.__lenis = lenis;
  }

  // ---------- Üst menü gizle/göster ----------
  const header = $("[data-header]");
  let lastY = 0;
  addEventListener("scroll", () => {
    const y = scrollY;
    header.classList.toggle("is-hidden", y > 200 && y > lastY && !document.body.classList.contains("menu-open"));
    lastY = y;
  }, { passive: true });

  // ---------- İmleç ----------
  const cursor = $("[data-cursor]"), cursorLabel = $("[data-cursor-label]");
  if (fine && cursor) {
    const xTo = G.quickTo(cursor, "x", { duration: .45, ease: "power3" });
    const yTo = G.quickTo(cursor, "y", { duration: .45, ease: "power3" });
    addEventListener("mousemove", e => { xTo(e.clientX); yTo(e.clientY); cursor.style.opacity = 1; });
    document.addEventListener("mouseleave", () => cursor.style.opacity = 0);
    document.addEventListener("mouseover", e => {
      const t = e.target.closest("[data-cursor-text]");
      const l = !t && e.target.closest("a, button, summary, label.chip");
      cursor.classList.toggle("is-label", !!t);
      cursor.classList.toggle("is-link", !!l);
      cursorLabel.textContent = t ? t.dataset.cursorText : "";
    });
  }

  // ---------- Mıknatıs butonlar ----------
  if (fine) $$(".magnetic").forEach(b => {
    const x = G.quickTo(b, "x", { duration: .6, ease: "power3" }), y = G.quickTo(b, "y", { duration: .6, ease: "power3" });
    b.addEventListener("mousemove", e => { const r = b.getBoundingClientRect(); x((e.clientX - r.left - r.width / 2) * .3); y((e.clientY - r.top - r.height / 2) * .3); });
    b.addEventListener("mouseleave", () => { x(0); y(0); });
  });

  // ---------- Sayfa geçişi (perde) ----------
  const curtain = $("[data-curtain]");
  // CSS'teki translateY(100%) GSAP'te piksel olarak okunuyor; yüzdeyle sıfırdan başlat.
  G.set(curtain, { y: 0, yPercent: 100 });
  document.addEventListener("click", e => {
    const a = e.target.closest("a");
    if (!a || e.metaKey || e.ctrlKey || e.shiftKey || a.target === "_blank" || a.hasAttribute("download")) return;
    const url = new URL(a.href, location.href);
    if (url.origin !== location.origin || url.pathname.startsWith("/yonetim") || (url.pathname === location.pathname && url.hash)) return;
    if (a.getAttribute("href").startsWith("mailto:") || a.getAttribute("href").startsWith("tel:")) return;
    e.preventDefault();
    closeMenu();
    G.timeline().set(curtain, { yPercent: 100, transformOrigin: "bottom" })
      .to(curtain, { yPercent: 0, duration: .7, ease: "expo.inOut" })
      .add(() => { sessionStorage.setItem("idmh-transition", "1"); location.href = url.href; });
  });
  addEventListener("pageshow", e => { if (e.persisted) G.set(curtain, { yPercent: 100 }); });

  // ---------- Metin bölme ----------
  const splitWords = el => {
    const out = [];
    const walk = node => [...node.childNodes].forEach(n => {
      if (n.nodeType === 3) {
        const frag = document.createDocumentFragment();
        n.textContent.split(/(\s+)/).forEach(part => {
          if (!part) return;
          if (/^\s+$/.test(part)) return frag.append(" ");
          const line = document.createElement("span"); line.style.cssText = "display:inline-block;overflow:hidden;vertical-align:top;padding-bottom:.06em;margin-bottom:-.06em";
          const w = document.createElement("span"); w.style.display = "inline-block"; w.textContent = part;
          line.append(w); frag.append(line); out.push(w);
        });
        n.replaceWith(frag);
      } else if (n.nodeType === 1 && n.tagName !== "BR") walk(n);
    });
    walk(el);
    return out;
  };
  const wordsOnly = el => {
    el.innerHTML = el.textContent.trim().split(/\s+/).map(w => `<span class="w">${w}</span>`).join(" ");
    return $$(".w", el);
  };

  // ---------- Giriş animasyonu ----------
  const intro = G.timeline({ paused: true });
  const rise = $$("[data-rise]");
  if (rise.length) intro.from(rise, { yPercent: 110, rotate: 3, duration: 1.4, stagger: .09 }, 0);
  $$("[data-hero], .project-hero, .page-head, .post-head").forEach(sec => {
    $$("[data-split]", sec).forEach(el => intro.from(splitWords(el), { yPercent: 110, duration: 1.3, stagger: .05 }, .05));
    $$("[data-fade]", sec).forEach((el, i) => intro.fromTo(el, { opacity: 0, y: 20 }, { opacity: 1, y: 0, duration: 1.2 }, .5 + i * .1));
  });
  $$(".project-hero-img img, .studio-hero img").forEach(img => intro.from(img, { scale: 1.2, duration: 2.2, ease: "power3.out" }, 0));

  const preloader = $("[data-preloader]");
  const firstVisit = !sessionStorage.getItem("idmh-visited");
  const fromTransition = sessionStorage.getItem("idmh-transition") === "1";
  sessionStorage.removeItem("idmh-transition");
  if (firstVisit && preloader) {
    sessionStorage.setItem("idmh-visited", "1");
    document.body.classList.add("is-loading");
    lenis?.stop();
    const count = $("[data-count]"), o = { v: 0 };
    G.timeline()
      .from(".preloader-mark", { yPercent: 100, duration: 1.2, ease: "expo.out" })
      .to(o, { v: 100, duration: 1.6, ease: "power2.inOut", onUpdate: () => count.textContent = Math.round(o.v) }, 0)
      .to(preloader, { yPercent: -100, duration: 1.1, ease: "expo.inOut" }, "+=.15")
      .add(() => { preloader.remove(); document.body.classList.remove("is-loading"); lenis?.start(); }, "-=.2")
      .add(() => intro.play(), "-=.75");
  } else {
    preloader?.remove();
    if (fromTransition) {
      G.set(curtain, { yPercent: 0 });
      G.to(curtain, { yPercent: -100, duration: .9, ease: "expo.inOut", delay: .05 });
      intro.delay(.35);
    }
    intro.play();
  }

  // ---------- Kaydırma animasyonları ----------
  $$("[data-split]").forEach(el => {
    if (el.closest("[data-hero], .project-hero, .page-head, .post-head")) return;
    G.from(splitWords(el), { yPercent: 110, duration: 1.3, stagger: .05, scrollTrigger: { trigger: el, start: "top 88%" } });
  });
  ST.batch("[data-fade-up]", { start: "top 90%", onEnter: els => G.fromTo(els, { opacity: 0, y: 60 }, { opacity: 1, y: 0, duration: 1.4, stagger: .1, overwrite: true }) });
  $$("[data-fade]").forEach(el => { if (!el.closest("[data-hero], .project-hero, .page-head, .post-head")) G.fromTo(el, { opacity: 0 }, { opacity: 1, duration: 1.2, scrollTrigger: { trigger: el, start: "top 90%" } }); });

  // Kelime kelime koyulaşan paragraflar
  $$("[data-words]").forEach(p => {
    const words = wordsOnly(p);
    ST.create({ trigger: p, start: "top 82%", end: "bottom 50%", scrub: true,
      onUpdate: s => { const n = Math.round(s.progress * words.length); words.forEach((w, i) => w.classList.toggle("on", i < n)); } });
  });

  // Perde gibi açılan görseller
  $$("[data-reveal]").forEach(el => G.fromTo(el, { clipPath: "inset(100% 0% 0% 0%)" }, {
    clipPath: "inset(0% 0% 0% 0%)", duration: 1.6, ease: "expo.inOut", scrollTrigger: { trigger: el, start: "top 85%" },
  }));
  $$("[data-reveal] img").forEach(img => G.from(img, { scale: 1.3, duration: 2, ease: "expo.out", scrollTrigger: { trigger: img.parentElement, start: "top 85%" } }));

  // Paralaks
  $$("[data-parallax]").forEach(img => G.fromTo(img, { yPercent: -7 }, { yPercent: 7, ease: "none", scrollTrigger: { trigger: img.closest(".ph") || img.parentElement, start: "top bottom", end: "bottom top", scrub: true } }));
  $$("[data-parallax-strong]").forEach(img => G.fromTo(img, { yPercent: -18 }, { yPercent: 0, ease: "none", scrollTrigger: { trigger: img.closest("section"), start: "top bottom", end: "bottom top", scrub: true } }));

  // Hero görseli: sabitlenip tam ekrana açılır
  const hv = $("[data-hero-visual]");
  if (hv) {
    G.timeline({ scrollTrigger: { trigger: hv, start: "top top", end: "+=120%", scrub: 1, pin: true } })
      .to(".hero-visual-inner", { clipPath: "inset(0% 0% 0% 0%)", ease: "none" }, 0)
      .to(".hero-visual img", { scale: 1, ease: "none" }, 0);
  }
  // Proje kapakları kaydırdıkça hafifçe büyür
  $$(".project-hero-img img").forEach(img => G.to(img, { yPercent: 12, ease: "none", scrollTrigger: { trigger: img.closest("section"), start: "top top", end: "bottom top", scrub: true } }));

  // Kayan yazı bandı: kaydırma hızına göre
  const mq = $("[data-marquee] .marquee-track");
  if (mq) {
    let x = 0, dir = -1, speed = 0;
    const width = () => mq.scrollWidth / 4;
    ST.create({ trigger: mq, start: "top bottom", end: "bottom top", onUpdate: s => { speed = Math.min(Math.abs(s.getVelocity()) / 400, 8); dir = s.direction === 1 ? -1 : 1; } });
    G.ticker.add(() => { x += dir * (0.6 + speed); speed *= .92; const w = width(); if (x < -w) x += w; if (x > 0) x -= w; G.set(mq, { x }); });
  }

  // Liste üzerinde imleci takip eden görsel
  $$("[data-hover-list]").forEach(list => {
    const box = list.parentElement.querySelector("[data-hover-img]");
    if (!box || !fine) return;
    const img = $("img", box);
    const xTo = G.quickTo(box, "x", { duration: .7, ease: "power3" }), yTo = G.quickTo(box, "y", { duration: .7, ease: "power3" });
    list.addEventListener("mousemove", e => { xTo(e.clientX); yTo(e.clientY); });
    $$("[data-img]", list).forEach(row => row.addEventListener("mouseenter", () => { img.src = row.dataset.img; G.to(box, { opacity: 1, scale: 1, duration: .5, ease: "power3.out" }); }));
    list.addEventListener("mouseleave", () => G.to(box, { opacity: 0, scale: .8, duration: .4, ease: "power3.out" }));
  });

  // Süreç: bölüm sabitlenir, adımlar ve görseller sırayla değişir
  const proc = $("[data-process]");
  if (proc) {
    const steps = $$(".process-step", proc), imgs = $$(".process-img", proc), bar = $("[data-process-bar]", proc);
    let current = -1;
    const show = i => {
      if (i === current) return;
      steps.forEach((s, k) => s.classList.toggle("is-on", k === i));
      imgs.forEach((im, k) => {
        if (k === 0) return;
        G.to(im, { clipPath: k <= i ? "inset(0% 0 0 0)" : "inset(100% 0 0 0)", duration: 1.1, ease: "expo.inOut", overwrite: true });
        G.to($("img", im), { scale: k <= i ? 1 : 1.2, duration: 1.4, ease: "expo.out", overwrite: true });
      });
      current = i;
    };
    show(0);
    ST.create({ trigger: proc, start: "top top", end: () => "+=" + innerHeight * (steps.length - 0.4), pin: ".process-pin", scrub: true,
      onUpdate: s => { G.set(bar, { scaleX: s.progress }); show(Math.min(steps.length - 1, Math.floor(s.progress * steps.length))); } });
  }

  // Footer'daki dev IDMH yazısı
  const fw = $("[data-footer-word]");
  if (fw) G.from(fw, { yPercent: 60, ease: "none", scrollTrigger: { trigger: fw.parentElement, start: "top bottom", end: "bottom bottom", scrub: true } });

  addEventListener("load", () => ST.refresh());
})();
