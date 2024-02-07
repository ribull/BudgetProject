import { IconDefinition } from '@fortawesome/fontawesome-svg-core';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { forwardRef } from 'react';

interface IconButtonProps {
  fontAwesomeIcon: IconDefinition;
  onClick: () => void;
}

const IconButton = forwardRef<HTMLButtonElement, IconButtonProps>(
  function IconButton({ fontAwesomeIcon, onClick }: IconButtonProps, ref) {
    return (
      <button
        type="button"
        className="icon-button"
        onClick={() => onClick()}
        ref={ref}
      >
        <FontAwesomeIcon icon={fontAwesomeIcon} />
      </button>
    );
  },
);

export default IconButton;
