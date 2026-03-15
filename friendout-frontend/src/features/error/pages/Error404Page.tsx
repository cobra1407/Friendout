import moon from "@/assets/images/moon.png";
import { ErrorLayout } from "../layouts/ErrorLayout";
import { getTranslation } from "@/i18n";

export const Error404Page = () => {
  const handleRedirect = () => {
    window.location.href = "/";
  };

  return (
    <ErrorLayout>
      <div className="text-white bg-red">
      {/* 404 */}
      <div className="flex items-center">
        <span className="mx-8 text-[10vw] font-bold">4</span>
        <img
          src={moon}
          alt={getTranslation('error404.icon_alt')}
          className="w-[150px] animate-float"
        />
        <span className="mx-8 text-[10vw] font-bold">4</span>
      </div>

      {/* Message */}
      <p className="mt-6 text-xl">
        {getTranslation('error404.message')}
      </p>

      {/* Button */}
      <button
        onClick={handleRedirect}
        className="
          mt-8
          h-[50px]
          w-[200px]
          rounded-md
          bg-gradient-to-br from-[#ff416c] to-[#ff4b2b]
          font-bold
          shadow-lg
          transition
          duration-300
          hover:-translate-y-1 hover:shadow-xl
          active:translate-y-0.5 active:shadow-md
        "
      >
        {getTranslation('error404.back_home')}
      </button>
      </div>
    </ErrorLayout>
  );
};
