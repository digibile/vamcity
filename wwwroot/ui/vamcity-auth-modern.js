/* Route-aware styling only: authentication, OTP, password login and registration logic remain unchanged. */
(function () {
  "use strict";
  function applyVamCityPageClass() {
    var path = (window.location.pathname || "/").toLowerCase();
    var isLogin = /^\/login(?:\/|$)/.test(path);
    var isRegister = /^\/register(?:\/|$)/.test(path);
    var isAgentRegister = /agentregister|agent\/register/.test(path);
    var isMembership = isRegister || isAgentRegister || /memberregister|stepformregister/.test(path);
    document.body.classList.toggle("vam-auth-page", isLogin || isRegister);
    document.body.classList.toggle("vam-membership-page", isMembership || isAgentRegister);
    document.body.classList.toggle("vam-agent-registration-page", isAgentRegister);
  }
  applyVamCityPageClass();
  window.addEventListener("popstate", applyVamCityPageClass);
  ["pushState", "replaceState"].forEach(function (method) {
    var original = window.history[method];
    if (typeof original !== "function") return;
    window.history[method] = function () {
      var result = original.apply(this, arguments);
      window.dispatchEvent(new Event("vamcity:routechange"));
      return result;
    };
  });
  window.addEventListener("vamcity:routechange", applyVamCityPageClass);
  window.addEventListener("load", applyVamCityPageClass, { once: true });
})();