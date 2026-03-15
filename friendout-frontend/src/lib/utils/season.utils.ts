export const getCurrentSeason = (): 'spring' | 'summer' | 'autumn' | 'winter' => {
  const now = new Date();
  const year = now.getFullYear();

  const spring = new Date(year, 2, 20);   // 20 march
  const summer = new Date(year, 5, 21);   // 21 june
  const autumn = new Date(year, 8, 23);   // 23 september
  const winter = new Date(year, 11, 21);  // 21 december

  if (now >= spring && now < summer) return 'spring';
  if (now >= summer && now < autumn) return 'summer';
  if (now >= autumn && now < winter) return 'autumn';

  return 'winter';
};
